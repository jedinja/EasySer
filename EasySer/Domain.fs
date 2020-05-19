module EasySer.Domain

type internal PageIndex = int
type internal RecordIndex = int

type internal RecordId = {
    Page: PageIndex
    Index: RecordIndex
}

let internal onlyPage p = { Page = p; Index = 0 }

type EasyCollection = {
    CollectionName: string
}

