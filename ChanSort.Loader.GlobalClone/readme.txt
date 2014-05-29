This module allows loading of LG's GlobalClone*.TLL files.
There seem to be different versions of this XML file format, depending on Series, Model and/or firmware version.

2013 LA-Series, 2014 LB55xx and LB56xx:
<vchName> is binary data inside an UTF8 envelope. Once decoded, this data is correct.
Some higher numbered models seem to exclusively support the GlobalClone and no longer have a binary TLL file,
while lower numbered models export both files and can only load the binary file.
<hexVchName> is not included in these models

2014 LB6xxx and higher:
<vchName> is a readable text, but sometimes left empty and misses all local characters
<hexVchName> may be present, depending on firmware. It contains a hex-encoded DVB encoded channel name with correct data.

