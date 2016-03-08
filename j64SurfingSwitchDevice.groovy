/**
 *  j64 - Channel Surf Switch
 *
 *  Copyright 2016 joejarvis64@gmail.com
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
 */
metadata {
	definition (name: "j64 - Surfing Switch", namespace: "j64", author: "joejarvis64@gmail.com") {
		capability "Switch"
	}

	simulator {
		// TODO: define status and reply messages here
	}

    tiles {
        standardTile("switch", "device.switch", width: 3, height: 3, canChangeIcon: true) {
            state "off", label: '${name}', action: "switch.on",  icon: "st.Health & Wellness.health2", backgroundColor: "#ffffff"
            state "on", label:  '${name}', action: "switch.off", icon: "st.Health & Wellness.health2",  backgroundColor: "#009933"
        }

        standardTile("refreshTile", "device.power", decoration: "ring") {
            state "default", label:'', action:"refresh.refresh", icon:"st.secondary.refresh"
        }

        standardTile("switchMain", "device.switch", width: 3, height: 3, canChangeIcon: true) {
            state "off", label: '${name}', action: "switch.on",  icon: "st.Health & Wellness.health2-icn", backgroundColor: "#ffffff"
            state "on", label:  '${name}', action: "switch.off", icon: "st.Health & Wellness.health2-icn",  backgroundColor: "#009933"
        }

        main "switchMain"
            details(["switch"])}
	}

// parse events into attributes
def parse(String description) {
	log.debug "Parsing '${description}'"
	// TODO: handle 'switch' attribute

}

// handle commands
def on() {
    log.debug "Channel Surf On"
    parent.ChannelSurfOn()
}

def off() {
    log.debug "Channel Surf Off"
    parent.ChannelSurfOff()
}