1.I open new api on dotnet 7
2.In the mission i understant that i have to remember all the register calls and also count how many read and write calls are made.
3.the api contains 2 post methods for register and unregister
4.the class controller will have the instance of the singelton that i create in the program.cs with the implemantation of dependency injection that dotnet core provide us.
5.The singelton is expend of an interface that contains also the 2 methods register and unregister.
6.Every time that register method has been called i will run the chmod command in "add mode",for example i will send read as true and the file has all ready read permission 
  nothing will change in the file permission.
  after the chomod was complete i need to add the call to the RequestsSaver dictionary that his key is the number that return to the client(like the index of the call) 
  and the value is all the properties of the request and isUnRegister prop that means if this call already unregister, by created is false.
  Also i have to knew how many times resd and write call are been made to the spesific file, for example i send to register 2 calls for the same file, the first one was read 
  and write and the secound wad only read.
  if i will call unregister(2) for canceled the secound call i should not remove the read permission from the file becouse it has two read calls, so only when i unregister(1)
  i will remove the read permission.
7.unregister was more tricky becouse there are a lot of options that can happend:
 *the call that i want to calncel contains read and write or only read or only write.
 *the call that i want to calncel contains read and write and the count of the read and write calls for the spesific file are more than 1 - so in this case i just decrese by 1
  the number of calls and no need to call chmod command.but if there are not more than 1 i have to run the chmod. And do so to all the other options.

It was difficult to find the right way to run terminal command from the project. i try to search a good solution for windows and mac so in the function i check witch enviroment 
i'm and add solution for both , but i cant check the windows becouse i'm working on mac.











