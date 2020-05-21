module EasyWebServer.Login

open System
open EasySerJson
open EasySerJson.Carrier
open EasyWebServer
open Suave
open Suave.Operators
open Suave.RequestErrors
open EasySer.EntityFactory

type Token = string
type UserRef = int
type LoggedInRecord = Token * UserRef

type private Session = {
    loggedIn: LoggedInRecord list
}

let private loggedInList (session: Carrier<Session>) = session.Record.loggedIn

let private factory = Factory defaultFileControllerConfig
let private SessionStore = factory.CreateEntity<Session> "_Session_Login"

let private AUTH_TOKEN = "auth-token"

let private tokenFromRequest (req: HttpRequest) =
    match req.header AUTH_TOKEN with
    | Choice1Of2 a -> a
    | Choice2Of2 _ -> ""

let private EnsureLogin sessionId = request (fun req ->
    let token = req |> tokenFromRequest
    if SessionStore.get sessionId.Id |> loggedInList |> List.exists (fun item -> fst item = token) then
        succeed
    else
        unauthorized Array.empty
    )

let private SetLoginToken = request (tokenFromRequest >> Writers.addHeader AUTH_TOKEN)

type TryLogin<'a> = 'a -> int option

let private addToSession token id sessionRecord =
    { sessionRecord with loggedIn = (token,id)::sessionRecord.loggedIn }
    
let private removeFromSession token sessionRecord =
    { sessionRecord with loggedIn = sessionRecord.loggedIn |> List.filter (fun item -> fst item <> token) }
    
let private getFromSession token sessionRecord =
    sessionRecord.loggedIn
    |> List.find (fun item -> fst item = token)
    |> snd
    
let private login<'loginModel> sessionId (tryLogin:TryLogin<'loginModel>) = request (fun req ->
    let model = Serializer.deserialize<'loginModel> (Mid.requestBody req)
    match tryLogin model with
    | None -> BAD_REQUEST "Wrong pass or name"
    | Some id ->
        let guid = Guid.NewGuid ()
        let token = guid.ToString ()
        let currentSession = SessionStore.get sessionId.Id
        SessionStore.update currentSession.Id (addToSession token id currentSession.Record) |> ignore
        Writers.addHeader AUTH_TOKEN token >=> Successful.OK "Good"
    )

let private logout sessionId = request (tokenFromRequest >> (fun token ->
    let currentSession = SessionStore.get sessionId.Id
    SessionStore.update currentSession.Id (removeFromSession token  currentSession.Record) |> ignore
    Successful.OK "Good"
    ))

let private withUserId sessionId apply = request (fun req ->
    let token = req |> tokenFromRequest
    let currentSession = SessionStore.get sessionId.Id
    apply req (getFromSession token currentSession.Record)
    )

type LoginInterface<'loginModel> = {
    ensureLogin: WebPart
    setLogin: WebPart
    login: 'loginModel -> WebPart
    logout: WebPart
    withUser: (HttpRequest -> UserRef -> WebPart) -> WebPart
}

let init () =
    let sessionId = SessionStore.create { loggedIn = [] }
    
    {
        ensureLogin = EnsureLogin sessionId
        setLogin = SetLoginToken
        login = login sessionId
        logout = logout sessionId
        withUser = withUserId sessionId
    }