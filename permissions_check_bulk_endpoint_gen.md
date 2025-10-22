POST /permissions/check-bulk
Content-Type: application/json
{
    "user_id": 123123,
    "permission": "credit_transfer_can_create",
    "object_type": "account",
    "object_ids": [ "67890", "67891" ]
}
Response: 200 OK
Content-Type: application/json
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
