This module allows loading of LG's GlobalClone*.TLL files.
There seem to be different versions of this XML file format around.

2014 LB-Series (e.g. Model LB731V, LB630V):
- vchName is a readable text, but sometimes left empty
- <hexVchName> contains hex-encoded channel name (not empty even when vchName is empty)
- <notConvertedLengthOfVchName>

2014 LB-Series (e.g. 42LB630V-ZA)
- vchName is a readable text
- no extra hex encoded version

2014 LB561V, LB580V
- vchName binary data (with high bit set)
- separate binary TLL file available
