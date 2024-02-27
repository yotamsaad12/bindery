1.  I opened a new API on .NET 7 
2.  From the mission I understood that I have to remember all the register calls and also count how many read and write calls are made. 
3.  The API contains 2 POST methods for register and unregister 
4.  The class controller will have the instance of the singleton that I created in the program.cs
    with the implemantation of dependency injection that dotnet core provide us. 
5.  The singleton extends an interface that contains the 2 methods for register and unregister. 
6.  Every time that register method has been called I will run the chmod command in "add mode", for example
    if I send read as true and the file has already read permission nothing will change in the file permission. 
    after the chmod is completed I need to add the call to the RequestsSaver dictionary that his key is the number that
    return to the client(like the index of the call) and the value is all the properties of the request and isUnRegister prop
    that means if this call already unregistered, by created is false. 
    Also I have to knew how many times resd and write call are been made to the spesific file, for example
    if I send to register 2 calls for the same file, the first one was read and write and the second wad only read.
    if I will call unregister(2) for canceled the second call I should not remove the read permission from the file
    because it has two read calls, so only when I unregister(1) I will remove the read permission. 
7.  unregister was more tricky because there are a lot of options that can happend: 
    *   the call that I want to cancel contains read and write or only read or only write. 
    *   the call that I want to calncel contains read and write and the count of the read and write calls for the spesific
        file are more than 1 - so in this case I just decrease by 1 the number of calls and no need to call chmod command.
        but if there are not more than 1 I have to run the chmod.
    *   And do so to all the other options.

It was difficult to find the right way to run terminal command from the project.
I try to search for a good solution for windows and mac so in the function I check which enviroment
I'm in and add solution for both , but I cant check the windows because i'm working on mac.