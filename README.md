# EasySer

A data access layer working with F# records, which serializes and saves on disk. 

No data integrity included. 

Useful for prototyping when you don't want to hassle with a full database and mappings to it.

## Usage

```fsharp
type TestModel = {
    Name: string
}

open EasySer.EntityFactory

let private fac = Factory(defaultFileControllerConfig)

let TEST_COLLECTION = "Test"
let Test = fac.CreateEntity<TestModel> TEST_COLLECTION
```

Now you have several functions on Test to save and extract data in the working directory
