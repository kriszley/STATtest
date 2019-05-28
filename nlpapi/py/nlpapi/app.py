from flask import Flask
import flask
import azure.cosmosdb.table.tableservice
import azure.batch
import uuid
import json

app = Flask(__name__)

tables = azure.cosmosdb.table.tableservice.TableService(connection_string="DefaultEndpointsProtocol=https;AccountName=nlptaskqueue;AccountKey=h/gC1nc70Ql6KENcoX6TDKeNvfJorzk1UKh7d480MQKPA+BKlW0cZMqqxewg4WW9/xFSCYZCLGx08iEW2OgF+Q==;EndpointSuffix=core.windows.net")

@app.route('/insert/<name>')
def insert(name):
  tables.insert_entity("bar", {"PartitionKey":"baz", "RowKey": "quux", "foo": "bar"})
  return "ok"

@app.route('/query')
def query():
  x = tables.get_entity("bar", "baz", "quux")
  return str(x)

@app.route('/create', methods=['POST'])
def create():
  # decode request
  data = flask.request.json
  service = data["service"]
  group = None
  if("group" in data):
    group = data["group"]
  callback = None
  if("callback" in data):
    callback = data["callback"]
  document = data["document"]
  documents = []
  for x in document:
    typ = x["type"]
    z = Document()
    z.type = "inline"
    z.part = x["part"]
    if(typ == "inline"):
      z.body = x["body"]
    elif(typ == "upload"):
      z.id = uuid.UUID(x.id)
    elif(typ == "url"):
      z.url = x["url"]
    elif(typ == "bsurl"):
      z.url = x["url"]
    else:
      raise ValueError("oops")
    documents.append(z)
  # add to table
  guidstr = uuid.uuid4()
  if(group is not None):
    tblobjgroup = {"PartitionKey":group, "RowKey":str(guidstr), "DocumentID": str(guidstr), "GroupID": group}
    tables.insert_entity("documentTaskGroupTable", tblobjgroup)
  tblobj = {"PartitionKey": str(guidstr), "RowKey": str(guidstr), "DocumentId":str(guidstr), "State": "creating", "Callback": callback, "Service": service}
  tables.insert_entity("documentTaskTable", tblobj)
  status = {"id": guidstr, "status": "creating"}
  return flask.jsonify(status)


@app.route("/status/<id>", methods=['GET'])
def status(id):
  guid = uuid.UUID(id)
  guidstr = str(guid)
  entity = tables.get_entity("documentTaskTable", guidstr, guidstr)
  if(entity.State == "complete"):
    status = {"id": guidstr, "status": entity.State, "output": {}}
  else:
    status = {"id": guidstr, "status": entity.State}
  return flask.jsonify(status)

@app.route("/cancel/<id>", methods=['GET'])
def cancel(id):

  #

@app.route("/list/<groupid>", methods=['GET'])
def list(groupid):
  #

class Document:
  def __repr__(self):
    return str(self.__dict__)
