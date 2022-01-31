from __future__ import print_function
import json
import sys
import requests
data = None
styles = None



url = 'http://localhost:8000/decos/'
x = requests.get(url)

# print(x.text)


with open('data.json') as json_file:
    data = json.load(json_file)
    
with open('styles.json') as json_file:
    styles = json.load(json_file)
# Convert JSON tree to a Python dict

# Convert back to JSON & print to stderr so we can verify that the tree is correct.
# print(json.dumps(data, indent=4), file=sys.stderr)

# Extract tree edges from the dict
edges = []

def get_edges(treedict, parent=None):
    name = next(iter(treedict.keys()))
    if parent is not None:
        edges.append((parent, name))
    for item in treedict[name]["children"]:
        if isinstance(item, dict):
            get_edges(item, parent=name)
        else:
            edges.append((name, item))

get_edges(data)

# Dump edge list in Graphviz DOT format
print('strict digraph tree {')
for s in styles:
    print(s)
    
for row in edges:
    print('    {0} -> {1};'.format(*row))

print('}')
