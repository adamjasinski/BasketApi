# Basket Web API Demo

[![N|NetCore](http://dotnetcore.io/wp-content/uploads/2017/01/DNC-Logo-300x226.png)](https://docs.microsoft.com/en-us/aspnet/core/)
Basket Web API is a demo of a RESTful API for managing shopping cart contents, written in C#/ASP .NET Core.
It uses [HAL](http://stateless.co/hal_specification.html) (Hypertext Application Language) convention for representing resources and links, making it easier to navigate to related resources.

## Features:
- Authenticate a user (please use any username with password ``demo`` ). The API uses Bearer JWT tokens.
- Retrieve basket contents
- Add an item to the basket
- Retrieve item contents from the basket
- Update item quantity in the basket
- Remove an item from the basket
- Remove all items from the basket

## Assumptions:
- Basket operations for unauthenticated users aren't supported
- Each authenticated user has a basket (and only one basket) - therefore baskets are identified by userid
- Items in the basket are identified by product id
- Adding another item with the same product id to the basket simply results in increasing of the quantity of existing item
- Only quantity can be updated in a basket item
- Product data is stubbed (test data is embedded in basket item contents for illustration purposes only)

## Projects in the solution
* BasketApi - Basket REST API itself (ASP .NET Core Web API)
* BasketApiClient - sample .NET client simplifying calls to Basket API
* BasketApi.Contracts - data contracts, shared between the server and the client project
* BasketApi.IntegrationTests - test scenarios exercising the client making calls to the API

# Authentication
Users need to authenticate with the Token endpoint.
Any username is accepted by the token endpoint, as long as the password is ``demo``.

#### HTTP
POST http://localhost:56128/api/token
Content-Type: application/json
``{ "username": "someuser", "password": "demo}``

Response:
``<JWT token content>``
or HTTP 401 if authentication wasn't successful.

#### BasketApiClient
```C#
_client.Authorize("someuser", "password")
```
Successful authentication stores the JWT token in client HTTP headers

# Basket operations
## HTTP
### Get basket for current user
Retrieves basket contents for currently authenticated user.
GET http://localhost:56128/api/my/basket
(Note: basket canonical URL is http://localhost:56128/api/users/<userid>/basket)
Authorization: Bearer <JWT token>
Response:
```
{
    "_links": {
        "self": {
            "href": "/api/users/cc1edc35-cbc1-40dd-abdb-73604dcc9fab/basket",
            "title": "basket"
        },
        "items": {
            "href": "/api/users/cc1edc35-cbc1-40dd-abdb-73604dcc9fab/basket/items",
            "title": "items"
        }
    },
    "id": "cc1edc35-cbc1-40dd-abdb-73604dcc9fab",
    "items": [
        {
            "_embedded": null,
            "_links": {
                "self": {
                    "href": "/api/users/cc1edc35-cbc1-40dd-abdb-73604dcc9fab/basket/items/61a63109-17f6-48aa-b942-366022abaa8d",
                    "title": "basket"
                }
            },
            "productId": "61a63109-17f6-48aa-b942-366022abaa8d",
            "quantity": 3
        }
    ],
    "expirationDate": "0001-01-01T00:00:00"
}
```

### Add basket item
POST http://localhost:56128/api/users/cc1edc35-cbc1-40dd-abdb-73604dcc9fab/basket/items
Authorization: Bearer <JWT token>
Content-Type: application/json
``{
    "productId": "61a63109-17f6-48aa-b942-366022abaa8d",
    "quantity": 3
}``

Response:
HTTP 201
Location: api/users/cc1edc35-cbc1-40dd-abdb-73604dcc9fab/basket/items/61a63109-17f6-48aa-b942-366022abaa8d

### Update basket item
PUT http://localhost:56128/api/users/cc1edc35-cbc1-40dd-abdb-73604dcc9fab/basket/items/61a63109-17f6-48aa-b942-366022abaa8d
Authorization: Bearer <JWT token>
Content-Type: application/json
``{
    "quantity": 3
}``

Response:
HTTP 200 if successful; HTTP 404 if item doesn't exist

### Get basket item
GET http://localhost:56128/api/users/cc1edc35-cbc1-40dd-abdb-73604dcc9fab/basket/items/61a63109-17f6-48aa-b942-366022abaa8d
Authorization: Bearer <JWT token>


Response:
HTTP 200 if successful; HTTP 404 if item doesn't exist
```
{
    "_embedded": {
        "product": {
            "_links": {
                "self": {
                    "href": "http://api.contoso.com/products/61a63109-17f6-48aa-b942-366022abaa8d",
                    "title": "product"
                }
            },
            "productId": "61a63109-17f6-48aa-b942-366022abaa8d",
            "description": "Description of product 61a63109-17f6-48aa-b942-366022abaa8d",
            "productImageUrl": "http://contoso.com/products/assets/61a63109-17f6-48aa-b942-366022abaa8d.png",
            "price": 75.5642617007551
        }
    },
    "_links": {
        "self": {
            "href": "/api/users/cc1edc35-cbc1-40dd-abdb-73604dcc9fab/basket/items/61a63109-17f6-48aa-b942-366022abaa8d",
            "title": "basket"
        }
    },
    "productId": "61a63109-17f6-48aa-b942-366022abaa8d",
    "quantity": 3
}
```

### Get all basket items
GET http://localhost:56128/api/users/cc1edc35-cbc1-40dd-abdb-73604dcc9fab/basket/items
Authorization: Bearer <JWT token>

Response:
HTTP 200
```
[
    {
        "_embedded": {
            "product": {
                "_links": {
                    "self": {
                        "href": "http://api.contoso.com/products/61a63109-17f6-48aa-b942-366022abaa8d",
                        "title": "product"
                    }
                },
                "productId": "61a63109-17f6-48aa-b942-366022abaa8d",
                "description": "Description of product 61a63109-17f6-48aa-b942-366022abaa8d",
                "productImageUrl": "http://contoso.com/products/assets/61a63109-17f6-48aa-b942-366022abaa8d.png",
                "price": 16.8330286242222
            }
        },
        "_links": {
            "self": {
                "href": "/api/users/cc1edc35-cbc1-40dd-abdb-73604dcc9fab/basket/items/61a63109-17f6-48aa-b942-366022abaa8d",
                "title": "basket"
            }
        },
        "productId": "61a63109-17f6-48aa-b942-366022abaa8d",
        "quantity": 3
    },
    {
        "_embedded": {
            "product": {
                "_links": {
                    "self": {
                        "href": "http://api.contoso.com/products/91a63109-17f6-48aa-b942-366022abaa8e",
                        "title": "product"
                    }
                },
                "productId": "91a63109-17f6-48aa-b942-366022abaa8e",
                "description": "Description of product 91a63109-17f6-48aa-b942-366022abaa8e",
                "productImageUrl": "http://contoso.com/products/assets/91a63109-17f6-48aa-b942-366022abaa8e.png",
                "price": 99.8433476313219
            }
        },
        "_links": {
            "self": {
                "href": "/api/users/cc1edc35-cbc1-40dd-abdb-73604dcc9fab/basket/items/91a63109-17f6-48aa-b942-366022abaa8e",
                "title": "basket"
            }
        },
        "productId": "91a63109-17f6-48aa-b942-366022abaa8e",
        "quantity": 10
    }
]
```

### Delete basket item
DELETE http://localhost:56128/api/users/cc1edc35-cbc1-40dd-abdb-73604dcc9fab/basket/items/61a63109-17f6-48aa-b942-366022abaa8d
Authorization: Bearer <JWT token>

Response:
HTTP 200 if successful; HTTP 404 if item doesn't exist

### Delete all basket items
DELETE http://localhost:56128/api/users/cc1edc35-cbc1-40dd-abdb-73604dcc9fab/basket/items
Authorization: Bearer <JWT token>

Response:
HTTP 200

# BasketApiClient
BasketApiClient is a .NET library, which can be used for calling Basket Web API.

## Authentication
```C#
var client = new BasketApiClient(new Uri("http://localhost:56128"));
client.Authorize("someuser", "password")
```
Successful authentication stores the JWT token in client HTTP headers

## Get basket for currently authenticated user
```C#
var basket = client.GetOwnBasket();
```

## Add basket item
Basket obtained in the previous step is needed to add an item:
```C#
var basketItemUrl = await _client.AddBasketItem(basket, basketItemToAdd);
```
## Update basket item
URL obtained in the previous step can be used to update an item:
```C#
await _client.UpdateBasketItem(basketItemUrl, basketItemToUpdate);
```

## Delete basket item
URL obtained in the previous step can be used to update an item:
```C#
await _client.DeleteBasketItem(basketItemUrl);
```

## Delete all basket items
Basket obtained in the previous step is needed to delete all items from the basket:
```C#
await _client.ClearBasket(basket);
```
Note: after clearing the basket, the client re-reads its contents from the API.

## Appendix
A collection of Postman requests - can be used for testing locally:
https://www.getpostman.com/collections/3069849a02c14d17f89b