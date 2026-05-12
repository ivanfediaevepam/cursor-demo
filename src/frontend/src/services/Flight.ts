/** Mirrors `AviationApi.Models.FlightStatus` for API payloads and UI. */
export type FlightStatus =
    | "Scheduled"
    | "Boarding"
    | "Departed"
    | "InAir"
    | "Landed"
    | "Cancelled"
    | "Delayed";

/** Mirrors `AviationApi.Models.DelayReasonCatalog.AllowedValues`. */
export type DelayReason = "Weather" | "Technical" | "Operational";

export const DELAY_REASON_OPTIONS: readonly DelayReason[] = [
    "Weather",
    "Technical",
    "Operational",
];

export const FLIGHT_STATUS_OPTIONS: readonly FlightStatus[] = [
    "Scheduled",
    "Boarding",
    "Departed",
    "InAir",
    "Landed",
    "Cancelled",
    "Delayed",
];

export function isFlightStatus(value: string): value is FlightStatus {
    return (FLIGHT_STATUS_OPTIONS as readonly string[]).includes(value);
}

export interface Flight {
    id: number;
    flightNumber: string;
    origin: string;
    destination: string;
    departureTime: Date;
    arrivalTime: Date;
    status: string;
    /** Present only when `status` is Delayed; otherwise null/undefined. */
    delayReason?: string | null;
    fuelRange: number;
    fuelTankLeak: boolean;
    flightLogSignature: string;
    aerobaticSequenceSignature: string;
}
