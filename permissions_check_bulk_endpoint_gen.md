POST /users/123123/check-bulk
Content-Type: application/json
[
    {
        "object_type": "functional-group",
        "object_ids": [ "banking_active" ],
        "permissions": [ "member" ]
    },
    {
        "object_type": "account",
        "object_ids": [ "67890", "67891" ],
        "permissions": [ "credit_transfer_can_create", "foreign_transfer_can_create" ]
    }
]
Response: 200 OK
Content-Type: application/json
[
    {
        "object_type": "account",
        "object_id": "67890",
        "permissions": "credit_transfer_can_create",
        "has_permission": true
    },
    {
        "object_type": "account",
        "object_id": "67891",
        "permissions": "credit_transfer_can_create",
        "has_permission": false
    },
    {
        "object_type": "account",
        "object_id": "67890",
        "permissions": "foreign_transfer_can_create",
        "has_permission": true
    },
    {
        "object_type": "account",
        "object_id": "67891",
        "permissions": "foreign_transfer_can_create",
        "has_permission": false
    },
    {
        "object_type": "functional-group",
        "object_id": "banking_active",
        "permissions": "member",
        "has_permission": true
    }
]

sp_permission_check_bulk @permission='credit_transfer_can_create' @user=123123 @object_type ='account' @object_ids = [ "67890", "67891" ]
@returns
[
    {
        "object_id": "67890",
        "has_permission": true
    },
    {
        "object_id": "67891",
        "has_permission": false
    }
]
