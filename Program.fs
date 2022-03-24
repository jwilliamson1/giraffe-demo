open System
open System.Security.Claims
open System.Threading
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Http.Features
open Microsoft.AspNetCore.Authentication
open Microsoft.Extensions.Hosting
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Giraffe
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open RipView
open PersonView

let authScheme = CookieAuthenticationDefaults.AuthenticationScheme

let accessDenied = setStatusCode 401 >=> text "Access Denied"

let mustBeUser (next : HttpFunc) (ctx : HttpContext) = requiresAuthentication accessDenied

let mustBeAdmin (next : HttpFunc) (ctx : HttpContext) =
    requiresAuthentication accessDenied
    >=> requiresRole "Admin" accessDenied

let mustBeJohn (next : HttpFunc) (ctx : HttpContext) =
    requiresAuthentication accessDenied
    >=> authorizeUser (fun u -> u.HasClaim (ClaimTypes.Name, "John")) accessDenied

let loginHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            let issuer = "http://localhost:5000"
            let claims =
                [
                    Claim(ClaimTypes.Name,      "John",  ClaimValueTypes.String, issuer)
                    Claim(ClaimTypes.Surname,   "Doe",   ClaimValueTypes.String, issuer)
                    Claim(ClaimTypes.Role,      "Admin", ClaimValueTypes.String, issuer)
                ]
            let identity = ClaimsIdentity(claims, authScheme)
            let user     = ClaimsPrincipal(identity)

            do! ctx.SignInAsync(authScheme, user)

            return! text "Successfully logged in" next ctx
        }


let webApp = 
    choose [
        route "/ping/rip" >=> (ripView { Name = "Magyar" } |> htmlView)
        route "/ping"   >=> text "pong"
        route  "/"     >=> (personView { Name = "Html Node" } |> htmlView)
        route  "/login"      >=> loginHandler
        ]


[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)

    builder.Services.AddAuthorization() |> ignore
    builder.Services.AddAuthentication().AddCookie() |> ignore
    let app = builder.Build()

    

    app.UseGiraffe webApp    
    app.UseAuthorization () |> ignore
    
    app.Run()

    0 // Exit code

