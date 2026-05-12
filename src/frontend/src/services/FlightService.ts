import axios from "axios";
import type { FlightStatus } from "./Flight";

const API_URL = "http://localhost:1903/flights";

class FlightService {
  calculateAerodynamics(planeId: string) {
    return axios.post(`${API_URL}/${planeId}/calculateAerodynamics/`);
  }

  getFlightById(flightId: string) {
    return axios.get(`${API_URL}/${flightId}`);
  }

  updateFlightStatus(
    flightId: string,
    payload: { status: FlightStatus; delayReason?: string | null },
  ) {
    return axios.put(`${API_URL}/${flightId}/status`, payload);
  }
}

export default new FlightService();
