import React, { useEffect, useRef, useState } from 'react';
import type { AxiosError } from 'axios';
import {
    DELAY_REASON_OPTIONS,
    FLIGHT_STATUS_OPTIONS,
    type DelayReason,
    type Flight,
    type FlightStatus,
    isFlightStatus,
} from '../services/Flight';
import FlightService from '../services/FlightService';
import FlightStatusBadge from './FlightStatusBadge';
import { Airplane } from './Airplane';

interface FlightDetailsProps {
    flight: Flight;
}

/** Normalizes dates when `flight` comes from JSON (e.g. axios) as ISO strings. */
function coerceFlight(f: Flight): Flight {
    return {
        ...f,
        departureTime:
            f.departureTime instanceof Date
                ? f.departureTime
                : new Date(String(f.departureTime)),
        arrivalTime:
            f.arrivalTime instanceof Date
                ? f.arrivalTime
                : new Date(String(f.arrivalTime)),
    };
}

function mapApiFlight(data: Record<string, unknown>): Flight {
    return {
        id: Number(data.id),
        flightNumber: String(data.flightNumber),
        origin: String(data.origin),
        destination: String(data.destination),
        departureTime:
            data.departureTime instanceof Date
                ? data.departureTime
                : new Date(String(data.departureTime)),
        arrivalTime:
            data.arrivalTime instanceof Date
                ? data.arrivalTime
                : new Date(String(data.arrivalTime)),
        status: String(data.status ?? ''),
        delayReason:
            data.delayReason == null || data.delayReason === ''
                ? null
                : String(data.delayReason),
        fuelRange: Number(data.fuelRange),
        fuelTankLeak: Boolean(data.fuelTankLeak),
        flightLogSignature: String(data.flightLogSignature ?? ''),
        aerobaticSequenceSignature: String(data.aerobaticSequenceSignature ?? ''),
    };
}

const FlightDetails: React.FC<FlightDetailsProps> = ({ flight }) => {
    const planeRef = useRef(null);
    const [displayFlight, setDisplayFlight] = useState<Flight>(() =>
        coerceFlight(flight),
    );
    const [selectedStatus, setSelectedStatus] = useState<FlightStatus>(() => {
        const f = coerceFlight(flight);
        return isFlightStatus(f.status) ? f.status : 'Scheduled';
    });
    const [selectedDelayReason, setSelectedDelayReason] = useState<
        '' | DelayReason
    >('');
    const [statusError, setStatusError] = useState<string | null>(null);
    const [statusPending, setStatusPending] = useState(false);

    useEffect(() => {
        setDisplayFlight(coerceFlight(flight));
    }, [flight]);

    useEffect(() => {
        if (isFlightStatus(displayFlight.status)) {
            setSelectedStatus(displayFlight.status);
        } else {
            setSelectedStatus('Scheduled');
        }
        const dr = displayFlight.delayReason;
        if (
            displayFlight.status === 'Delayed' &&
            dr &&
            (DELAY_REASON_OPTIONS as readonly string[]).includes(dr)
        ) {
            setSelectedDelayReason(dr as DelayReason);
        } else {
            setSelectedDelayReason('');
        }
    }, [displayFlight]);

    const onSimulateAerobaticSequence = () => {};

    const delaySubmitDisabled =
        selectedStatus === 'Delayed' && selectedDelayReason === '';

    const onStatusChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
        const next = e.target.value as FlightStatus;
        setSelectedStatus(next);
        if (next !== 'Delayed') {
            setSelectedDelayReason('');
        }
        setStatusError(null);
    };

    const onSubmitStatus = async (e: React.FormEvent) => {
        e.preventDefault();
        setStatusError(null);
        setStatusPending(true);
        try {
            const payload: { status: FlightStatus; delayReason?: string } = {
                status: selectedStatus,
            };
            if (selectedStatus === 'Delayed') {
                payload.delayReason = selectedDelayReason;
            }
            const { data } = await FlightService.updateFlightStatus(
                String(displayFlight.id),
                payload,
            );
            setDisplayFlight(mapApiFlight(data as Record<string, unknown>));
        } catch (err) {
            const ax = err as AxiosError<string>;
            const body = ax.response?.data;
            const msg =
                typeof body === 'string' && body.length > 0
                    ? body
                    : 'Unable to update status.';
            setStatusError(msg);
        } finally {
            setStatusPending(false);
        }
    };

    const statusLine = isFlightStatus(displayFlight.status) ? (
        <FlightStatusBadge
            status={displayFlight.status}
            delayReason={displayFlight.delayReason}
        />
    ) : (
        <span>{displayFlight.status}</span>
    );

    return (
        <div>
            <div className="absolute w-52 h-52 top-32 right-32" ref={planeRef}>
                <Airplane />
            </div>
            <p className="text-amber-900 text-lg leading-6 font-serif">Flight Number: {displayFlight.flightNumber}</p>
            <p className="mt-6 text-amber-900 text-lg leading-6 font-serif">Aerobatic Sequence Signature: <strong>{displayFlight.aerobaticSequenceSignature}</strong></p>
            <button className="mt-3 bg-amber-900 text-white font-bold py-2 px-4 rounded cursor-pointer" onClick={onSimulateAerobaticSequence}>Simulate Aerobatic Sequence</button>
            <p className="mt-6 text-amber-900 text-lg leading-6 font-serif">Origin: {displayFlight.origin}</p>
            <p className="mt-6 text-amber-900 text-lg leading-6 font-serif">Destination: {displayFlight.destination}</p>
            <p className="mt-6 text-amber-900 text-lg leading-6 font-serif">Departure Time: {displayFlight.departureTime.toString()}</p>
            <p className="mt-6 text-amber-900 text-lg leading-6 font-serif">Arrival Time: {displayFlight.arrivalTime.toString()}</p>
            <p className="mt-6 text-amber-900 text-lg leading-6 font-serif flex flex-wrap items-center gap-2">
                <span>Status:</span>
                {statusLine}
            </p>

            <form
                className="mt-8 max-w-md space-y-3 rounded border border-amber-200 bg-amber-50/50 p-4"
                onSubmit={onSubmitStatus}
            >
                <p className="font-serif text-sm font-semibold text-amber-900">Update flight status</p>
                <div>
                    <label htmlFor="flight-status-select" className="block text-xs font-medium text-amber-800">New status</label>
                    <select
                        id="flight-status-select"
                        className="mt-1 block w-full rounded border border-amber-300 bg-white px-2 py-1.5 text-sm text-amber-950"
                        value={selectedStatus}
                        onChange={onStatusChange}
                    >
                        {FLIGHT_STATUS_OPTIONS.map((s) => (
                            <option key={s} value={s}>{s}</option>
                        ))}
                    </select>
                </div>
                {selectedStatus === 'Delayed' ? (
                    <div>
                        <label htmlFor="delay-reason-select" className="block text-xs font-medium text-amber-800">Delay reason (required)</label>
                        <select
                            id="delay-reason-select"
                            className="mt-1 block w-full rounded border border-amber-300 bg-white px-2 py-1.5 text-sm text-amber-950"
                            value={selectedDelayReason}
                            onChange={(e) => {
                                setSelectedDelayReason(e.target.value as DelayReason | '');
                                setStatusError(null);
                            }}
                        >
                            <option value="">Select reason…</option>
                            {DELAY_REASON_OPTIONS.map((r) => (
                                <option key={r} value={r}>{r}</option>
                            ))}
                        </select>
                    </div>
                ) : null}
                {statusError ? (
                    <p className="text-sm text-red-700" role="alert">{statusError}</p>
                ) : null}
                <button
                    type="submit"
                    className="rounded bg-amber-800 px-3 py-1.5 text-sm font-semibold text-white disabled:cursor-not-allowed disabled:opacity-50"
                    disabled={delaySubmitDisabled || statusPending}
                >
                    {statusPending ? 'Saving…' : 'Save status'}
                </button>
            </form>

            <p className="mt-6 text-amber-900 text-lg leading-6 font-serif">Fuel Range: {displayFlight.fuelRange}</p>
            <p className="mt-6 text-amber-900 text-lg leading-6 font-serif">Fuel Tank Leak: {displayFlight.fuelTankLeak ? 'Yes' : 'No'}</p>
            <p className="mt-6 text-amber-900 text-lg leading-6 font-serif">Flight Log Signature: {displayFlight.flightLogSignature}</p>
        </div>
    );
};

export default FlightDetails;
