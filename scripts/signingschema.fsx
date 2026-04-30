#load "common.fsx"

open Common
open Google.Protobuf.WellKnownTypes
open System.Linq

let amount = 5455.5
let users = ["a"; "e"]

let signingGroupsForUsers users: string list = 
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
    (signingGroupsForUsers users)
        .Join(creditTransferSigningGroupsForAccount "a1" amount, id, id, fun res subj -> res)
    |> Seq.map (fun res -> 
        (res.Split('|').[2]))
    |> Seq.groupBy id
    |> Seq.map (fun (group, instances) -> (group, instances.Count()))
    |> Seq.toList

let canSend =
    let context = Struct()
    context.Fields.Add("amount", Value.ForNumber(amount))
    let achieved_signatures = Struct()

    for (group, count) in achievedSignatures do
        achieved_signatures.Fields.Add(group, Value.ForNumber(float count))

    context.Fields.Add("achieved_signatures", Value.ForStruct(achieved_signatures))

    checkPermission "credit_transfer_can_send" "account" "a1" "user" "a" context
