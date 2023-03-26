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

and a `defs` folder containing ***SOME*** protos. please remember, this tool isnt perfect and may not parse all of them perfectly\
![image](https://user-images.githubusercontent.com/84511935/227778873-0c40ea86-3dae-4214-8614-52cf15677eed.png)

9- profit


# KNOWN ISSUES:

- some maps may not be parsed correctly

- oneof's are not working

