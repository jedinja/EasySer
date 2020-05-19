module EasySer.ListFunctions

let rec internal getByIndex index list =
    match list with
    | [] -> failwith ("Couldn't find element with index " + index.ToString())
    | h::t -> match index with
                | 0 -> h
                | _ -> getByIndex (index-1) t