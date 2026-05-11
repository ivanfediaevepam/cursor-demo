/** Mirrors `AviationApi.Models.FlightStatus` for API payloads and UI. */
export type FlightStatus =
    | "Scheduled"
    | "Boarding"
    | "Departed"
    | "InAir"
    | "Landed"
    | "Cancelled"
    | "Delayed";

export interface Flight {
    id: number;
    flightNumber: string;
    origin: string;
    destination: string;
    departureTime: Date;
    arrivalTime: Date;
    status: string;
    fuelRange: number;
    fuelTankLeak: boolean;
    flightLogSignature: string;
    aerobaticSequenceSignature: string;
}