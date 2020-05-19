module EasySer.FileController

open EasySer.Domain
open EasySer.IO
open EasySer.ListFunctions
open EasySerJson.Serializer
open EasySerJson.Carrier

type FileControllerConfig = {
    BaseDir: string
    PageFileSize: int
    PageFilePrefix: string
}

let internal toRecordId config id =
    { Page = (id / config.PageFileSize); Index = (id % config.PageFileSize) }
    
let internal fromRecordId config id =
    config.PageFileSize * id.Page + id.Index
    
let private getPageFileName config id =
    config.PageFilePrefix + id.Page.ToString()

let private readFromCollection config collectionName id =
    let pageFileName = getPageFileName config id
    fullPath [ config.BaseDir; collectionName; pageFileName ] |> readFile
    
let rec private readAllFromCollection config collectionName startPage =
    let page = match startPage with
                | None -> onlyPage 0
                | Some i -> i
    let records =
        page
        |> readFromCollection config collectionName
    if records.Length < config.PageFileSize then
        records
    else
        List.concat [records; (readAllFromCollection config collectionName (Some { page with Page = page.Page+1 }))]

let internal loadFromCollection<'a> config (col:EasyCollection) id =
    readFromCollection config col.CollectionName id
    |> getByIndex id.Index
    |> deserialize<'a>
    |> toCarrier (fromRecordId config id)
    
let internal loadPageFromCollection<'a> config (col:EasyCollection) page =
    let pg = { Page = page; Index = 0 }
    readFromCollection config col.CollectionName pg
    |> List.map deserialize<'a>
    |> List.mapi (fun i item -> toCarrier (fromRecordId config { pg with Index = i }) item)

let private getLastPage config collectionName =
    fullPath [ config.BaseDir; collectionName ] |> findLastPage config.PageFilePrefix

let private getLastIndexInPage config collectionName page =
    let fileName = getPageFileName config { Page = page; Index=0}
    fullPath [ config.BaseDir; collectionName; fileName ]
    |> findLastIndex

let private createRecordInFile item path filename =
    let existing = [ path; filename ] |> fullPath |> readFile
    let newLine = serialize item

    List.append existing [newLine]
    |> writeFile path filename

let private getNewId config collectionName =
    let lastPage = getLastPage config collectionName
    {
        Page = lastPage
        Index = getLastIndexInPage config collectionName lastPage
    }
    |> fromRecordId config
    |> (+) 1
    |> toRecordId config 

let internal createRecordInCollection<'a> config (col:EasyCollection) (item:'a) =
    let newId = getNewId config col.CollectionName
    
    newId
    |> getPageFileName config
    |> createRecordInFile item (fullPath [config.BaseDir; col.CollectionName ])

    item
    |> toCarrier (fromRecordId config newId)
  
let private writeToCollection config collectionName id records =
    let pageFileName = getPageFileName config id
    let filename = fullPath [ config.BaseDir; collectionName; ]
    records
    |> writeFile filename pageFileName
    
let internal deleteFromCollection config (col:EasyCollection) id =
    readFromCollection config col.CollectionName id
    |> List.mapi (fun i record -> match i with
                                    | ind when ind.Equals id.Index -> ""
                                    | _ -> record)
    |> writeToCollection config col.CollectionName id

let internal updateRecordInCollection<'a> config (col:EasyCollection) id (item:'a) =
    readFromCollection config col.CollectionName id
    |> List.mapi (fun i record -> match i with
                                    | ind when ind.Equals id.Index -> serialize item
                                    | _ -> record)
    |> writeToCollection config col.CollectionName id
    
    item |> toCarrier (fromRecordId config id)
    
let internal getAll<'a> config col optionalSelector =
    readAllFromCollection config col.CollectionName None
    |> List.map deserialize<'a>
    |> List.mapi (fun i item ->
                                toCarrier i item)
    |> List.filter (fun item ->
                                match optionalSelector with
                                | None -> true
                                | Some predicate -> predicate item.Record)
