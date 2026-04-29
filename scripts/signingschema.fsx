#load "common.fsx"

open Common
open Google.Protobuf.WellKnownTypes
open System.Linq

let signingGroups users: string list = 
    let context = Struct()
    context.Fields.Add("now", Value.ForString("2024-06-15T12:00:00Z"))

    users
    |> List.collect (fun userId ->
        lookupResourcesForUser userId "can_sign" "signing_group" context)
    |> List.map (fun r -> r.ResourceObjectId)

let creditTransferGroups amount =
    let context = Struct()
    context.Fields.Add("amount", Value.ForNumber(amount))

    lookupSubjects "account" "a1" "credit_transfer_signing_group" "signing_group" context
    |> List.map (fun r -> r.Subject.SubjectObjectId)

(signingGroups ["a"; "b"; "e"])
    .Join(creditTransferGroups 300, id, id, fun res subj -> res, subj)
    .GroupBy(fun (res, subj) -> res)
    .Select(fun g -> (g.Key, g.Count()))
|> Seq.toList