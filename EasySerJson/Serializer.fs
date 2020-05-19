module EasySerJson.Serializer

open Newtonsoft.Json

let serialize obj = obj |> JsonConvert.SerializeObject

let deserialize<'a> str = str |> JsonConvert.DeserializeObject<'a>
