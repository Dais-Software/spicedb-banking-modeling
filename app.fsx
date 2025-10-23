#r "nuget: Authzed.Net, 1.5.0"

open Authzed.Api
open Authzed.Api.V1

open Google.Protobuf.WellKnownTypes;
open Grpc.Core;
open Grpc.Net.Client;
open System.Threading.Tasks

let createAuthzedClient () =
    let credentials = CallCredentials.FromInterceptor(fun context metadata ->
        metadata.Add("Authorization", "Bearer somerandomkeyhere")
        Task.CompletedTask
    )
    let options = new GrpcChannelOptions(
        Credentials = ChannelCredentials.Create(ChannelCredentials.Insecure, credentials),
        UnsafeUseInsecureChannelCallCredentials = true
    )
    let channel = Grpc.Net.Client.GrpcChannel.ForAddress("http://localhost:50051", options)
    PermissionsService.PermissionsServiceClient channel

let client = createAuthzedClient ()

let lookupSubjectsForAccount accountId =
    let lookupSubjRq = LookupSubjectsRequest(
        Resource = ObjectReference(
            ObjectType = "account",
            ObjectId = accountId
        ),
        Permission = "credit_transfer_can_create",
        SubjectObjectType = "user",
        Consistency = Consistency(FullyConsistent = true)
    )
    let lookupSubjRs = client.LookupSubjects(lookupSubjRq)

    lookupSubjRs.ResponseStream.ReadAllAsync().ToBlockingEnumerable() |> Seq.toList

let lookupResourcesForUser permission resourceObjectType userId context =
    let lookupResRq = LookupResourcesRequest(
        Subject = SubjectReference(
            Object = ObjectReference(
                ObjectType = "user",
                ObjectId = userId
            )
        ),
        Permission = permission,
        ResourceObjectType = resourceObjectType,
        Consistency = Consistency(FullyConsistent = true),
        Context = context
    )
    let lookupResRs = client.LookupResources(lookupResRq)

    lookupResRs.ResponseStream.ReadAllAsync().ToBlockingEnumerable() |> Seq.toList


let readRelationshipsForResource (resourceType: string option) (resourceId: string option) (subjectType: string option) (subjectId: string option) =
    let filter = RelationshipFilter()
    match resourceType with
    | Some rt -> filter.ResourceType <- rt
    | None -> ()
    match resourceId with
    | Some rid -> filter.OptionalResourceId <- rid
    | None -> ()
    match subjectType, subjectId with
    | Some st, Some sid ->
        filter.OptionalSubjectFilter <- SubjectFilter(SubjectType = st, OptionalSubjectId = sid)
    | Some st, None ->
        filter.OptionalSubjectFilter <- SubjectFilter(SubjectType = st)
    | None, _ -> ()

    let readRelsRq = ReadRelationshipsRequest(
        Consistency = Consistency(FullyConsistent = true),
        RelationshipFilter = filter
    )
    let readRelsRs = client.ReadRelationships(readRelsRq)
    readRelsRs.ResponseStream.ReadAllAsync().ToBlockingEnumerable() |> Seq.toList


let readRelationshipsForAccount accountId =
    readRelationshipsForResource (Some "account") (Some accountId) None None


let readRelationshipsForUser userId =
    readRelationshipsForResource None None (Some "user") (Some userId)


let checkPermissionForUser permission resourceType resourceId subjectType subjectId context =
    let checkPerRq = CheckPermissionRequest(
        Consistency = Consistency(FullyConsistent = true),
        Permission = permission,
        Resource = ObjectReference(
            ObjectType = resourceType,
            ObjectId = resourceId
        ),
        Subject = SubjectReference(
            Object = ObjectReference(
                ObjectType = subjectType,
                ObjectId = subjectId
            )
        ),
        Context = context
    )
    let checkPerRs = client.CheckPermission(checkPerRq)
    checkPerRs.Permissionship

let checkBulkPermissions items =
    let checkBulkRq = CheckBulkPermissionsRequest(
        Consistency = Consistency(FullyConsistent = true)
    )
    for (resourceType, resourceId, permission, subjectType, subjectId) in items do
        checkBulkRq.Items.Add(CheckBulkPermissionsRequestItem (
            Resource = ObjectReference(
                ObjectType = resourceType,
                ObjectId = resourceId
            ),
            Permission = permission,
            Subject = SubjectReference(
                Object = ObjectReference(
                    ObjectType = subjectType,
                    ObjectId = subjectId
                )
            )
        ))
    client.CheckBulkPermissions(checkBulkRq)


let subjects = lookupSubjectsForAccount "a1"
let resources = lookupResourcesForUser "credit_transfer_can_create" "account" "a" null
let functionalities = lookupResourcesForUser "member" "functional_group" "a" null


let signingGroups = 
    let context = Struct()
    context.Fields.Add("now", Value.ForString("2024-06-15T12:00:00Z"))

    lookupResourcesForUser "member" "signing_group" "av" context


let relationships = readRelationshipsForAccount "a1"
let userRelationships = readRelationshipsForUser "a"

let documentRelationships = readRelationshipsForResource (Some "document_rights") (Some "a1") None None

documentRelationships.[0].Relationship.OptionalCaveat.Context.Fields.["required_signatures"].ListValue.Values

let canSign = 
    let context = Struct()
    context.Fields.Add("now", Value.ForString("2025-10-22T12:00:00Z"))
    checkPermissionForUser "can_sign" "credit_transfer" "p1" "user" "av" context


let canSend =
    let context = Struct()
    context.Fields.Add("amount", Value.ForNumber(333))
    let achieved_signatures = Struct()
    achieved_signatures.Fields.Add("g1", Value.ForNumber(2.0))
    context.Fields.Add("achieved_signatures", Value.ForStruct(achieved_signatures))

    checkPermissionForUser "can_send" "credit_transfer" "p1" "user" "av" context

let permissionship = checkPermissionForUser "credit_transfer_can_create" "account" "a1" "user" "a" null
let permissionship_func = checkPermissionForUser "member" "functional_group" "banking_active" "user" "a"
let checkBulkRs = checkBulkPermissions [("account", "a1", "credit_transfer_can_create", "user", "a")]
