POST /accounts/permissions/lookup-resources
POST /clients/permissions/lookup-resources
POST /function-groups/permissions/lookup-resources
или
POST /accounts/permissions/credit_transfer_can_create/lookup-resources
Content-Type: application/json
{
    "user_id": 123123,
    "permission": "credit_transfer_can_create"
}
Response: 200 OK
Content-Type: application/json
[
    {
        "account_id": 67890,
        "has_permission": true
    }
]

sp_permission_lookup_resources_credit_transfer_can_create @user=123123
или
sp_permission_lookup_resources @permission='credit_transfer_can_create' @user=123123

@returns
[
    {
        "account_id": 67890,
        "has_permission": true
    }
]
