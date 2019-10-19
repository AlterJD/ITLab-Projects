namespace ITLab.Projects



module JWT =
    open System
    open System.Text
    open System.Security.Claims
    open System.IdentityModel.Tokens.Jwt
    open Microsoft.Extensions.Configuration
    open Microsoft.IdentityModel.Tokens

    let debugUserId = Guid.Parse("B2485D96-904C-4A79-8B95-60432BFDE828")


    let issuerSigningKey (configuration: IConfiguration) =
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetValue<string>("JWT:DebugKey")))

    let debugJwtToken (configuration: IConfiguration) =
        let key = issuerSigningKey configuration
        let credentials = SigningCredentials(key, SecurityAlgorithms.HmacSha256)

        let claims = [| 
            Claim(ClaimTypes.NameIdentifier, debugUserId.ToString()) 
            |]

        let jwt = new JwtSecurityToken(signingCredentials=credentials, claims=claims)
        JwtSecurityTokenHandler().WriteToken(jwt)
