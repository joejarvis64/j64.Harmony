/**
 *  j64Harmony
 *
 *  Copyright 2016 Joe Jarvis
 *
 *  Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except
 *  in compliance with the License. You may obtain a copy of the License at:
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 *  Unless required by applicable law or agreed to in writing, software distributed under the License is distributed
 *  on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License
 *  for the specific language governing permissions and limitations under the License.
 *
 *  Last Updated : 1/15/2016
 *
 */
definition(
	name: "j64 Harmony",
	namespace: "j64",
	author: "Joe Jarvis",
	description: "Control your harmony hub using j64 Harmony Server",
	category: "SmartThings Labs",
        iconUrl: "http://cdn.device-icons.smartthings.com/Electronics/electronics8-icn@2x.png",
        iconX2Url: "http://cdn.device-icons.smartthings.com/Electronics/electronics8-icn@2x.png",
        iconX3Url: "http://cdn.device-icons.smartthings.com/Electronics/electronics8-icn@3x.png"
)

mappings {
    path("/prepInstall") {
        action: [
            POST: "prepInstall",
        ]
    }
}

/* ****************** */
/* Prepare to install */
/* ****************** */
def prepInstall() {
	// Save info about the j64 server
    state.j64AppId = params.j64AppId
    state.j64User = params.j64UserName
    state.j64Password = params.j64Password

	// Find the first hub with an IP address
    def firstHub = location.hubs.find { it -> it.localIP != null }
    
    // If not found, bail now
    if (firstHub == null) {
    	log.debug "Could not find a hub with an IP address"
	    return [hubIP: "null", hubPort: "null"] 
    }
    
	def localIP =   "${firstHub.localIP}"
	def localPort = "${firstHub.localSrvPortTCP}"
    
    log.debug "Returned SmartThings IP/Port: ${localIP}:${localPort}"

    return [hubIP: "${localIP}", hubPort: "${localPort}"] 
}

/* *************** */
/* Install Methods */
/* *************** */
def installAllDevices(j64Devices) {
	def children = getChildDevices()

	// Add the various devices
	j64Devices.each { j64d ->
    	def dt = ""
        def ni = ""
        if (j64d.DeviceType == 0) {
        	dt = "j64 - Volume Switch"
            ni = "j64HarmonyVolume"
        }
        
        if (j64d.DeviceType == 1) {
        	dt = "j64 - Surfing Switch"
            ni = "j64HarmonySurfing"
        }
        
        if (j64d.DeviceType == 2) {
        	dt = "j64 - Channel Switch"
            ni = "j64HarmonyChannel-${j64d.DeviceValue}"
        }
        
        if (j64d.DeviceType == 3) {
        	dt = "j64 - VCR Switch"
            ni = "j64HarmonyVcr-${j64d.DeviceValue}"
        }
       	AddDevice(ni, j64d.Name, dt, children) 
    }
    
	// Remove any devices that were deleted from configuration
    def l = []
    children.each { c ->
        def found = j64Devices.find { j64d -> 
                    def ni = ""
                    if (j64d.DeviceType == 0) { ni = "j64HarmonyVolume"  }
                    if (j64d.DeviceType == 1) { ni = "j64HarmonySurfing" }
                    if (j64d.DeviceType == 2) { ni = "j64HarmonyChannel-${j64d.DeviceValue}" }
                    if (j64d.DeviceType == 3) { ni = "j64HarmonyVcr-${j64d.DeviceValue}" }
			        
                    ni == c.device.deviceNetworkId }
                    
    	if (found == null) {
        	l.push(c.device.deviceNetworkId)
        }
    }
    
    l.each { dnid -> 
    	try {
        	deleteChildDevice(dnid) 
        	log.debug "Removed device: ${dnid} because it was not in the j64 harmony config"
        } catch (Exception e) {
        	log.debug "Could NOT remove device: ${dnid}.  Is it still referenced by the Echo App?"
        }
	}
}

def AddDevice(networkId, name, deviceType, children) {

	def channelDevice = children.find { item -> item.device.deviceNetworkId == networkId }
    if (channelDevice == null) {
    	log.debug "Adding device ${name}"
		channelDevice = addChildDevice("j64", deviceType, networkId, null, [name: "${name}", label:"${name}"])
	}
    else {
     	log.debug "Updating device ${name}"
        channelDevice.name = name
        channelDevice.label = name
	}
}

/* *********************************** */
/* Routines to call j64 Harmony Server */
/* *********************************** */
def Mute() {
	hubApiGet("/api/HarmonyHub/ToggleMute")
}

def SetVolume(level) {
	hubApiGet("/api/HarmonyHub/SetVolume/${level}")
}

def VcrCommand(command) {
	hubApiGet("/api/HarmonyHub/Transport/${command}")
}

def SetChannel(channel) {
	hubApiGet("/api/HarmonyHub/SetChannel/${channel}")
}

def ChannelSurfOn() {
	hubApiGet("/api/HarmonyHub/ChannelSurf/Start")
}

def ChannelSurfOff() {
	hubApiGet("/api/HarmonyHub/ChannelSurf/Stop")
}

/* ************** */
/* Initialization */
/* ************** */
def installed() { 
	initialize() 
}

def updated() { 
	unsubscribe()
  	unschedule()
	initialize() 
}

def uninstalled() {
	unschedule()
}	

def initialize() {
	subscribe(location, null, localLanHandler, [filterEvents:false])
}

def refresh() {   
}

/* ************************************** */
/* Handle event from the j64HarmonyServer */ 
/* on the local LAN                       */
/* ************************************** */
def localLanHandler(evt) {

	// Only handle messages from the j64HarmonyServer
	def msg = parseLanMessage(evt.description)
	if (msg.json == null) {
	    return
    }
    
    // It must have the same App ID that was passed during the install
    if (msg.json.j64AppId != state.j64AppId) {
        return
    }
    
 	// Call the appropriate method based on the Route Parameter
    if (msg.json.Route == "/installAllDevices")
    {
    	state.j64Server = msg.json.j64Ip;
        state.j64Port = msg.json.j64Port;
       	installAllDevices(msg.json.Payload)
    }
}

/* **************** */
/* Helper Functions */
/* **************** */
private hubApiGet(apiPath) {	

	def userpassascii = "${state.j64User}:${state.j64Password}"
	def userpass = "Basic " + userpassascii.encodeAsBase64().toString()
    
    log.debug "Send request to ${j64HarmonyServerAddress()}" 
    def headers = [:] 
    headers.put("HOST", j64HarmonyServerAddress())
    //headers.put("Authorization", userpass)

	def result = new physicalgraph.device.HubAction(
 		   	method: "GET",
    		path: apiPath,
    		headers: headers
		)
        
    sendHubCommand(result)
}

private j64HarmonyServerAddress() {
	return state.j64Server + ":" + state.j64Port
}
