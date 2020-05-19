module EasySer.IO

open System.IO

let internal fullPath array = array |> String.concat "/"

let internal writeFile path filename data =
    Directory.CreateDirectory(path) |> ignore
    File.WriteAllText (fullPath [path; filename], data |> String.concat "\n")
    ()

let internal readFile filename =
    printfn "file exists %s %b" filename (File.Exists(filename))
    match File.Exists(filename) with
    | true -> filename |> File.ReadLines |> List.ofSeq
    | false -> []

let private extractIndex (prefix:string) (str:string) =
    Path.GetFileName(str).[(prefix.Length)..] |> int

let internal findLastPage prefix dir =
    if Directory.Exists dir then Directory.GetFiles(dir, prefix+"*") else [||]
    |> List.ofArray
    |> function
    | [] -> 0
    | l -> List.maxBy (extractIndex prefix) l |> extractIndex prefix

let private subsOne n = n - 1

let internal findLastIndex filename =
    filename |> readFile |> List.length |> subsOne