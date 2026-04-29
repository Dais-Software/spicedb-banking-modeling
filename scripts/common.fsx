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

let lookupSubjects objectType objectId permission subjectObjectType context =
    let lookupSubjRq = LookupSubjectsRequest(
        Resource = ObjectReference(
            ObjectType = objectType,
            ObjectId = objectId
        ),
        Permission = permission,
        SubjectObjectType = subjectObjectType,
        Consistency = Consistency(FullyConsistent = true),
        Context = context
    )
    let lookupSubjRs = client.LookupSubjects(lookupSubjRq)

    lookupSubjRs.ResponseStream.ReadAllAsync().ToBlockingEnumerable()
    |> Seq.toList

let lookupSubjectsForAccount accountId=
    lookupSubjects "account" accountId "credit_transfer_can_create" "user" null

let lookupResources objectType objectId permission resourceObjectType  context =
    let lookupResRq = LookupResourcesRequest(
        Subject = SubjectReference(
            Object = ObjectReference(
                ObjectType = objectType,
                ObjectId = objectId
            )
        ),
        Permission = permission,
        ResourceObjectType = resourceObjectType,
        Consistency = Consistency(FullyConsistent = true),
        Context = context
    )
    let lookupResRs = client.LookupResources(lookupResRq)

    lookupResRs.ResponseStream.ReadAllAsync().ToBlockingEnumerable()
    |> Seq.toList

let lookupResourcesForUser userId permission resourceObjectType  context =
    lookupResources "user" userId permission resourceObjectType context

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


let checkPermission permission resourceType resourceId subjectType subjectId context =
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
