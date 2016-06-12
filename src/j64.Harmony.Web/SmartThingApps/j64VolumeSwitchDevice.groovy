/**
 *  j64 - Volume Switch
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
	definition (name: "j64 Volume Switch", namespace: "j64", author: "joejarvis64@gmail.com") {
		capability "Switch"
		capability "Switch Level"
		capability "Refresh"
        }

	simulator {
	}

    tiles(scale: 2) {
    	multiAttributeTile(name:"switch", type: "lighting", width: 4, height: 2, canChangeIcon: true){
			tileAttribute ("device.switch", key: "PRIMARY_CONTROL") {
				attributeState "on", label:'Toggle Mute', action:"switch.off", icon:"st.Electronics.electronics12", backgroundColor:"#89c35c", nextState:"off"
				attributeState "off", label:'Toggle Mute', action:"switch.on", icon:"st.Electronics.electronics12", backgroundColor:"#89c35c", nextState:"on"
				attributeState "turningOn", label:'Toggling Mute', action:"switch.off", icon:"st.Electronics.electronics12", backgroundColor:"#89c35c", nextState:"turningOff"
				attributeState "turningOff", label:'Toggling Mute', action:"switch.on", icon:"st.Electronics.electronics12", backgroundColor:"#89c35c", nextState:"turningOn"
			}
			tileAttribute ("device.level", key: "SLIDER_CONTROL") {
				attributeState "level", action:"switch level.setLevel"
			}
		}
    
		valueTile("level", "device.level", inactiveLabel: false, decoration: "flat", width: 6, height: 2) {
			state "level", label: 'Level ${currentValue}%'
		}
        
        standardTile("refresh", "device.power", decoration: "ring", width: 3, height: 2) {
            state "default", label:'', action:"refresh.refresh", icon:"st.secondary.refresh"
        }

    	standardTile("switchMain", "device.switch", width: 2, height: 2, canChangeIcon: true) {
            state "on", label:  'Mute', action: "switch.off", icon: "st.Electronics.electronics12-icn",  backgroundColor: "#89c35c"
            state "off", label: 'Mute', action: "switch.on",  icon: "st.Electronics.electronics12-icn", backgroundColor: "#89c35c"
        }

	main "switchMain"
            details(["switch", "level"])}
	}

// parse events into attributes
def parse(String description) {
	log.debug "Parsing '${description}'"
}

// handle commands
def on() {
	log.debug "Toggle Mute"
    
    def currentLevel = device.currentValue("level")
    if (currentLevel == 0)
		sendEvent(name: "level", value: 15)

    parent.Mute()
}

def off() {
	log.debug "Toggle Mute"
    parent.Mute()
}

def setLevel(value) {
	log.debug "Set level ${value}"
	value = value as Integer
	if (value == 0) {
		off()
		sendEvent(name: "switch", value: "off")
		sendEvent(name: "level", value: value)
	}
	else {
		sendEvent(name: "level", value: value)
		sendEvent(name: "switch", value: "on")
	}
    parent.SetVolume(value)
}

def refresh () {
}

