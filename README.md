# Sample .NET REST API MediatR implementation with Dapper,JWT and OpenApi using Clean Architecture.

About the repoitory:
Open source API written in the latest version of .NET, implementing the concepts of S.O.L.I.D, Clean Code and MediatR

# **Technologies**
- .NET 8
- MediatR
- FluentValidation
- JWT
- Swagger UI
- HealthChecks
- Data Layer - SQL Server compatible 
- Caching (Memory and Redis)
- Datadog compatible
- Sentry compatible
- Multilanguage Culture and Localization support

# **Architecture**
- SOLID 
- Clean Code
- Domain Driven Design (Layers and Domain Model Pattern)

# How to Run
## Clone this repo

> git clone https://github.com/yigitnuhuz/dotnet-sample-mediatr-api.git


## Prerequirements
This project based on .NET 8, please install corresponding SDK for your operating system:

After installing, please run below command to make sure current .NET version is 8 or above.
`dotnet --version`

## Run the API
### 1. App will automaticly redirects to Swagger UI:

>If http => http://localhost:5153/swagger
>If https => https://localhost:7184/swagger 

---


### 2. Check API up and running

>curl -X 'GET' \
'http://localhost:5153/' \
-H 'accept: text/plain' \
-H 'APICulture: en-GB'

it suppose to be return Healthy response with 200 status code.

>{
"data": "Healthy",
"version": "v0.0.1"
}

---


# Authentication
## How to get valid JWT Token
Create API request with following Curl;
>curl -X 'POST' \
'http://localhost:5153/api/v1/Auth/login' \
-H 'accept: text/plain' \
-H 'APICulture: en-GB' \
-H 'Content-Type: application/json' \
-d '{
"username": "YigitNuhuz",
"password": "CustomPasswordForApi"
}'

You will get token response;
>{
"type": "Bearer",
"token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJTeXN0ZW0iOiJEb3RuZXRTYW1wbGVBcGkiLCJJc0F1dGhlbnRpY2F0ZWQiOiJUcnVlIiwiVXNlcklkIjoiMWYzYmZhMjktZmZlNy00OTM0LThkZDQtNTlkYzUxMjc1YjJhIiwiVXNlck5hbWUiOiJZaWdpdE51aHV6IiwiU2Vzc2lvbklkIjoiNWJlZDA4ZDMtNjU2Zi00ZWZjLWEzZmUtMGQwMWIwMGNjYmE4IiwiUHJvdmlkZXIiOiJDQVJCT04iLCJuYmYiOjE3MDUwNjY2NzksImV4cCI6MTcwNTA3MDI3OSwiaWF0IjoxNzA1MDY2Njc5fQ.HPQdBzixZMxe5KgCT-JK0Jk7FiPDbdu9xe832ixwt_0",
"expireIn": 60
}

# Validate token
There is a custom endpoint for this documentation. So you can make a request with your JWT token and you can get a your UserName from your token. 

Create API request with following Curl;
>curl -X 'GET' \
'http://localhost:5153/api/v1/Auth/validate' \
-H 'accept: */*' \
-H 'APICulture: en-GB' \
-H 'Authorization: bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJTeXN0ZW0iOiJEb3RuZXRTYW1wbGVBcGkiLCJJc0F1dGhlbnRpY2F0ZWQiOiJUcnVlIiwiVXNlcklkIjoiMWYzYmZhMjktZmZlNy00OTM0LThkZDQtNTlkYzUxMjc1YjJhIiwiVXNlck5hbWUiOiJZaWdpdE51aHV6IiwiU2Vzc2lvbklkIjoiNWJlZDA4ZDMtNjU2Zi00ZWZjLWEzZmUtMGQwMWIwMGNjYmE4IiwiUHJvdmlkZXIiOiJDQVJCT04iLCJuYmYiOjE3MDUwNjY2NzksImV4cCI6MTcwNTA3MDI3OSwiaWF0IjoxNzA1MDY2Njc5fQ.HPQdBzixZMxe5KgCT-JK0Jk7FiPDbdu9xe832ixwt_0'
>

You supposed to get the following response;
>your token is valid YigitNuhuz



