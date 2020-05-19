module EasySer.EntityFactory

open EasySer.Domain
open EasySer.FileController

type Entity<'a> (col: string, config: FileControllerConfig) =
    member this.Config = config
    member this.Collection = { CollectionName = col }
    member this.get id =
        id
        |> toRecordId this.Config
        |> loadFromCollection<'a> this.Config this.Collection
    member this.list page = loadPageFromCollection<'a> this.Config this.Collection page
    member this.update id item =
        updateRecordInCollection<'a> this.Config this.Collection (toRecordId this.Config id) item
    member this.create item = createRecordInCollection<'a> this.Config this.Collection item
    member this.delete id = deleteFromCollection this.Config this.Collection (toRecordId this.Config id)
    member this.all optPredicate = getAll<'a> this.Config this.Collection optPredicate
    
type Factory (config) =
    member this.Config = config
    member this.CreateEntity<'a> col = new Entity<'a>(col, this.Config)

let defaultFileControllerConfig = {
    BaseDir = "."
    PageFileSize = 100
    PageFilePrefix = "page"
}