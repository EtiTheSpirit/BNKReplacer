# BNKReplacer
A tool to replace existing sound files within Wwise BNK audio archives.

# Capabilities:
BNKReplacer **CAN**...
* Replace existing WEM files present inside of a BNK file with substitute WEM files you specify.
* Replace existing WEM files with basically anything. Hell, pack a meme in for a sound for all I care. It'll break the game. But hey, it works.
* **(Via BNKMerger, see Releases V2.0 and after)** Merge multiple BNK files that are mods of the same main BNK file with different contents to create a singular BNK file with both changes, as well as resolve conflicts between files if necessary.

BNKReplacer **CAN NOT**...
* Add new files to a BNK. Sorry. That requires some modification of other parts of the BNK file that isn't documented. :(
* Remove files from the BNK. Same reason as above.

# Usage:
## Step 1: Make a file and a folder in the same directory as the EXE.
![Setup](https://i.imgur.com/GfhZM8H.png)

***

## Step 2: Write a list of IDs in the text document. These IDs should be the IDs of files within the BNK file you're going to modify.

**Use a tool like No Man's Audio Suite to get ahold of these IDs!** https://github.com/monkeyman192/No-Man-s-Audio-Suite

**To create WEM files, use Wwise's tools! Here's a great video tutorial: https://www.youtube.com/watch?v=39Oeb4GvxEc**

### Method #1:
![Method 1](https://i.imgur.com/Wpsfy8k.png)

### Method #2:
![Method 2](https://i.imgur.com/8Tj0KNz.png)

(You can mix and match these methods too!)

## Step 3: Run the EXE

When you run the EXE, it will prompt you for the input and output file destinations. You can alternatively run it via a command line. The first argument is the input file, the second is the output file. If you suck with command lines, just double click the file and it'll guide you through.

# Developers:
All code is fully documented so ideally working with the code should be relatively straightforward. The only horribly messy parts are where console writing is done.
