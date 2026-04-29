#load "common.fsx"

open Common
open Google.Protobuf.WellKnownTypes

let subjects = lookupSubjectsForAccount "a1"
let resources = lookupResourcesForUser "a" "credit_transfer_can_create" "account" null
let functionalities = lookupResourcesForUser "a" "member" "functional_group" null


let signingGroups = 
    let context = Struct()
    context.Fields.Add("now", Value.ForString("2024-06-15T12:00:00Z"))

    lookupResourcesForUser "a" "member" "signing_group" context


let relationships = readRelationshipsForAccount "a1"
let userRelationships = readRelationshipsForUser "a"

let documentRelationships = readRelationshipsForResource (Some "document_rights") (Some "a1") None None

documentRelationships.[0].Relationship.OptionalCaveat.Context.Fields.["required_signatures"].ListValue.Values

let canSign = 
    let context = Struct()
    context.Fields.Add("now", Value.ForString("2025-10-22T12:00:00Z"))
    checkPermission "can_sign" "credit_transfer" "p1" "user" "av" context


let canSend =
    let context = Struct()
    context.Fields.Add("amount", Value.ForNumber(333))
    let achieved_signatures = Struct()
    achieved_signatures.Fields.Add("g1", Value.ForNumber(2.0))
    context.Fields.Add("achieved_signatures", Value.ForStruct(achieved_signatures))

    checkPermission "can_send" "credit_transfer" "p1" "user" "av" context

let permissionship = checkPermission "credit_transfer_can_create" "account" "a1" "user" "a" null
let permissionship_func = checkPermission "member" "functional_group" "banking_active" "user" "a"
let checkBulkRs = checkBulkPermissions [("account", "a1", "credit_transfer_can_create", "user", "a")]
