POST /accounts/permissions/check-bulk
POST /clients/permissions/check-bulk
POST /function-groups/permissions/check-bulk
или
POST /accounts/permissions/credit_transfer_can_create/check-bulk
Content-Type: application/json
{
    "user_id": 123123,
    "permission": "credit_transfer_can_create",
    "account_ids": [ 67890, 67891 ]
}
Response: 200 OK
Content-Type: application/json
[
    {
        "account_id": 67890,
        "has_permission": true
    },
    {
        "account_id": 67891,
        "has_permission": false
    }
]

sp_permission_check_bulk_credit_transfer_can_create @user=123123 @account_ids = [ 67890, 67891 ]
или
sp_permission_check_bulk @permission='credit_transfer_can_create' @user=123123 @account_ids = [ 67890, 67891 ]

@returns
[
    {
        "account_id": 67890,
        "has_permission": true
    },
    {
        "account_id": 67891,
        "has_permission": false
    }
]
