Ubb.Integration.Permissions
Ubb.Integration.UserPermissions
Ubb.Integration.UserRights
Ubb.Integration.AuthZ


POST /users/123123/clients/check-bulk
POST /clients/permissions/check-bulk


POST /users/123123/functional-groups/check-bulk
POST /functional-groups/permissions/check-bulk
Content-Type: application/json
{
    "user": 123123,
    "permission": "member",
    "functional-groups": [ "banking_active", "banking_passive ]
}
Response: 200 OK
Content-Type: application/json
[
    {
        "functional-group": "banking_active",
        "has_permission": false
    },
    {
        "functional-group": "banking_passive",
        "has_permission": true
    }
]

POST /users/123123/accounts/check-bulk-permissions
POST /accounts/permissions/check-bulk
или
POST /accounts/permissions/credit_transfer_can_create/check-bulk
Content-Type: application/json
{
    "user": 123123,
    "permission": [ "credit_transfer_can_create", "foreign_transfer_can_create" ],
    "accounts": [ 67890, 67891 ]
}
[
    {
        "permission": "credit_transfer_can_create",
        "account": 67890
    },
    {
        "permission": "credit_transfer_can_create",
        "account": 67891
    },
    {
        "permission": "foreign_transfer_can_create",
        "account": 67890
    },
    {
        "permission": "foreign_transfer_can_create",
        "account": 67891
    }
]
[
    {
        "account": 67890,
        "permissions": ["credit_transfer_can_create", "foreign_transfer_can_create"]
    },
    {
        "account": 67891,
        "permissions": ["credit_transfer_can_create", "foreign_transfer_can_create"]
    }
]

Response: 200 OK
Content-Type: application/json
[
    {
        "account": 67890,
        "has_permission": true
    },
    {
        "account": 67891,
        "has_permission": false
    }
]

sp_permission_accounts_check_bulk_credit_transfer_can_create @user=123123 @accounts = [ 67890, 67891 ]
или
sp_permission_accounts_check_bulk @permission='credit_transfer_can_create' @user=123123 @accounts = [ 67890, 67891 ]

@returns
[
    {
        "account": 67890,
        "has_permission": true
    },
    {
        "account": 67891,
        "has_permission": false
    }
]
