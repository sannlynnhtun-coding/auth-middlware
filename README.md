- hash (salt) sha256 / md5
- encrypt / decrypt [ aes, triple desc ]
- encode / decode
- serialize / deserialize
- jwt
- local storage / session storage

password

session
cookie
httpcontext
url

session

.net framework Session["user"] = user;

User user = Session["uesr"] as User; // object accept in server memory // string only accept in db

.net framework Session["user"] = user.ToJson();

User user = Session["user"].ToString().ToObject<User>();

HttpContext.Session.SetString("user", user.ToJson());

asp.net session id [client cookie] = session server [memory]

sigin => pass => response cookie add => 

request / response

sigin => pass => response header add => 

request

https://www.c-sharpcorner.com/article/compute-sha256-hash-in-c-sharp/

https://www.thetechplatform.com/post/cookies-in-asp-net-core

{
 amount : 1500,
 id : 1
 hash : dfmadfmoemwofmwefmofwf
}

amount:1000|id:1 = hash client

amount:1500|id:1 = hash server
