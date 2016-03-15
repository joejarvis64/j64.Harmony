/**
 *  j64 - Vcr Switch
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
	definition (name: "j64 VCR Switch", namespace: "j64", author: "joejarvis64@gmail.com") {
		capability "Switch"
		capability "Refresh"
        }

	simulator {
	}

    tiles {
        standardTile("switch", "device.switch", width: 3, height: 3, canChangeIcon: true) {
            state "off", label: '${name}', action: "switch.on",  icon: "st.Electronics.electronics5", backgroundColor: "#ffffff"
            state "on", label:  '${name}', action: "switch.off", icon: "st.Electronics.electronics5",  backgroundColor: "#009933"
        }
    
    	standardTile("switchMain", "device.switch", width: 2, height: 2, canChangeIcon: true) {
            state "off", label: '${name}', action: "switch.on",  icon: "st.Electronics.electronics5-icn", backgroundColor: "#ffffff"
            state "on", label:  '${name}', action: "switch.off", icon: "st.Electronics.electronics5-icn",  backgroundColor: "#009933"
        }
    
        main "switchMain"
            details(["switch"])}
	}

// parse events into attributes
def parse(String description) {
	log.debug "Parsing '${description}'"
}

// handle commands
def on() {
	runCommand()
}

def off() {
	runCommand()
}

def runCommand() {
	def command = device.deviceNetworkId.replaceAll("j64HarmonyVcr-","")
	log.debug "Set vcr command to ${command}"
	parent.VcrCommand(command)
}