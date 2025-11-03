
POST /users/123123/lookup-resources
Content-Type: application/json
[
    {
        "object_type": "functional-group",
        "permissions": [ "member" ]
    },
    {
        "object_type": "account",
        "permissions": [ "credit_transfer_can_create", "foreign_transfer_can_create" ]
    }
]
Response: 200 OK
Content-Type: application/json
[
    {
        "functional-group": "banking_active",
        "permission": "member",
        "has_permission": true
    },
    {
        "account": 67890,
        "permission": "credit_transfer_can_create",
        "has_permission": true
    }
]

sp_permission_lookup_resources_credit_transfer_can_create @user=123123
или
sp_permission_lookup_resources @permission='credit_transfer_can_create' @user=123123

@returns
[
    {
        "account": 67890,
        "has_permission": true
    }
]
