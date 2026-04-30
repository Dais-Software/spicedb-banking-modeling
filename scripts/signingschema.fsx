#load "common.fsx"

open Common
open Google.Protobuf.WellKnownTypes
open System.Linq

let amount = 5455.5
let users = ["a"; "e"]

let canSignSigningGroupsForUsers users: string list = 
    let context = Struct()
    context.Fields.Add("now", Value.ForString("2024-06-15T12:00:00Z"))

    users
    |> List.collect (fun userId ->
        lookupResourcesForUser userId "can_sign" "signing_group" context)
    |> List.map (fun r -> r.ResourceObjectId)

let creditTransferSigningGroupsForAccount accountId amount =
    let context = Struct()
    context.Fields.Add("amount", Value.ForNumber(amount))

    lookupSubjects "account" accountId "credit_transfer_signing_group" "signing_group" context
    |> List.map (fun r -> r.Subject.SubjectObjectId)

let achievedSignatures = 
    query {
        for usersSigningGroup in canSignSigningGroupsForUsers users do
        join accountsSigningGroup in creditTransferSigningGroupsForAccount "a1" amount 
            on (usersSigningGroup = accountsSigningGroup)
        groupBy (usersSigningGroup.Split('|').[2]) into g
        select (g.Key, g.Count())
    }
    |> Seq.toList

let canSend =
    let context = Struct()
    context.Fields.Add("amount", Value.ForNumber(amount))
    let achieved_signatures = Struct()

    for (group, count) in achievedSignatures do
        achieved_signatures.Fields.Add(group, Value.ForNumber(float count))

    context.Fields.Add("achieved_signatures", Value.ForStruct(achieved_signatures))

    checkPermission "credit_transfer_can_send" "account" "a1" "user" "a" context
