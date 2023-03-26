# Dumpcs2Protobuf
some shit i tried to make to extract the obfuscated protobuf messages from dump.cs file (needs to set manually the repetitive string that comes after the class name)

# YOU WILL NEED A FULL DUMP.CS FILE CONTAINING 100% FULL DUMP (INCLUDING ALL FULL FIELDS AND VALUES) ELSE IT WONT WORK


# USAGE

1- edit the "string searchString" in the code to match the repetitive string that comes after the class name

2- build it via Visual Studio 2022

3- put the dump.cs file ***NAMED AS "dump.cs"*** in same folder as the executable

4- run the program

5- you will be left with an `output` folder, containing the protobuf messages as `.cs` files, looking like this:\
![image](https://user-images.githubusercontent.com/84511935/227775430-5f95f1bd-c4ce-45c8-824a-7bdd2ce2b9b7.png)

6- next step will be making some script (manual work will just take forever) to parse them to actual `.proto` files.\
if you look closely you will see that the .cs files have the field name and type on the bottom part and the upper part will contain the field numbers.\
you will have to make a script that will make it something like this:\
![image](https://user-images.githubusercontent.com/84511935/227775731-b4d4b704-b6dd-48b0-b4f5-05c6c788fe97.png)

7- now just add the `proto3` syntax and `java_package` option (in case you using grasscutter) and you will have a ready proto:\
![image](https://user-images.githubusercontent.com/84511935/227775828-ac06e312-1328-4120-a987-4e657888e858.png)

8- all you have to do now is to deobfuscate it. do it manually or using some tools, im not going to guide you that.

9- profit
