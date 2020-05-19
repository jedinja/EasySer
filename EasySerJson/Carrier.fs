module EasySerJson.Carrier

type Carrier<'a> = {
    Id: int
    Record: 'a
}

let toCarrier<'a> id (record:'a) = {
    Id = id
    Record = record
}