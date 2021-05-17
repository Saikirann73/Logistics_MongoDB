#!/usr/bin/python3

from flask import Flask, request, send_from_directory, jsonify, abort

import json

app = Flask(__name__, static_url_path="")

"""

Plane

 {
    "callsign": "CARGO1234",
    "currentLocation": [ 55.2, -18.3],
    "heading": 165,
    "route": ["London","Paris"],
    "landed": "Madrid",
}

City
{
    "name": "London",
    "country": "United Kingdom",
    "location": [ 52.0, -0.1 ],
}

Cargo
{
    "id": "abcdabcdabcd",
    "destination": "Paris",
    "location": "Berlin",
    "courier": "CARGO1234",
    "received": "2020-06-15T11:34:05Z,
    "status": "in process" OR "delivered" 
}

"""


# Anything that isn't part of the API treat as a static file
# To be clear we aren't rendering anything server side - this is a rich
# Client app and web services

#Return the App with no URL - anything that isn't a route is returned as a file
#from the 'Static' directory by default

@app.route("/")
def root():
    return app.send_static_file("index.html")

#Get All City Info
@app.route("/cities", methods=["GET"])
def get_all_cities():
    return {"ok":False}

#Get infor for a specific City

@app.route("/cities/<id>", methods=["GET"])
def get_city_info(id):
    return {"ok":False}

#Get all Plane info

@app.route("/planes", methods=["GET"])
def get_all_planes():
    return {"ok":False}

#Get a Specific Plane

@app.route("/planes/<id>", methods=["GET"])
def get_plane_info(id):
    return {"ok":False}

#Update Location and Heading for a Plane 

@app.route("/planes/<id>/location/<location>/<heading>", methods=["PUT"])
def update_plane_location(id, location, heading):
    return {"ok":False}
#Update Location and Heading for a Plane  incluing an airport

@app.route("/planes/<id>/location/<location>/<heading>/<city>", methods=["PUT"])
def update_plane_location_city(id, location, heading, city):
    return {"ok":False}

# Remove the current routed destination (Pop from list)

@app.route("/planes/<id>/route/destination", methods=["DELETE"])
def remove_plane_destination(id):
    return {"ok":False}


# Replace the whole Route with a new Single destination
@app.route("/planes/<id>/route/<city>", methods=["PUT"])
def replace_plane_route(id, city):
    return {"ok":False}




# Add a a the  destination to the Route
@app.route("/planes/<id>/route/<city>", methods=["POST"])
def add_plane_destination(id, city):
    return {"ok":False}


# Add a completely new cargo at a location with a destination

@app.route("/cargo/<location>/to/<destination>", methods=["POST"])
def new_cargo(location, destination):
    return {"ok":False}


# Flag a specific cargo as having been delivered
@app.route("/cargo/<id>/delivered", methods=["PUT"])
def mark_delivered(id):
    return {"ok":False}

# Assign Cargo Courier to a cargo - it will move to them on arrival

@app.route("/cargo/<id>/courier/<courier>", methods=["PUT"])
def assign_courier(id, courier):
    return {"ok":False}

# Remove Cargo Courier - Remove the specified courier from a cargo (usually on arrival and offloading)
@app.route("/cargo/<id>/courier", methods=["DELETE"])
def remove_courier(id):
    return {"ok":False}


# Move Cargo - Change the location of a cargo, this should check it's not teleporting
@app.route("/cargo/<id>/location/<location>", methods=["PUT"])
def update_location(id, location):
    return {"ok":False}


# Get all cargo at a given location (Plane or City)
@app.route("/cargo/location/<location>", methods=["GET"])
def get_cargo(location):
    return {"ok":False}


# If we run this standalone not in gunicorn or similar
if __name__ == "__main__":
    app.run()
