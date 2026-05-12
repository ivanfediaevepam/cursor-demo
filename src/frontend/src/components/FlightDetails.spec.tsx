import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import FlightDetails from './FlightDetails';

const updateFlightStatusMock = vi.fn();

vi.mock('../services/FlightService', () => ({
  default: {
    updateFlightStatus: (...args: unknown[]) => updateFlightStatusMock(...args),
    getFlightById: vi.fn(),
    calculateAerodynamics: vi.fn(),
  },
}));

const baseFlight = {
  id: 1,
  flightNumber: 'WB001',
  origin: 'Kitty Hawk, NC',
  destination: 'Manteo, NC',
  departureTime: new Date('1903-12-17T10:35:00'),
  arrivalTime: new Date('1903-12-17T10:47:00'),
  status: 'Scheduled',
  fuelRange: 100,
  fuelTankLeak: false,
  flightLogSignature: 'sig',
  aerobaticSequenceSignature: 'aero',
};

describe('FlightDetails', () => {
  beforeEach(() => {
    updateFlightStatusMock.mockReset();
  });

  it('shows delay reason on the status badge when flight is delayed', () => {
    render(
      <FlightDetails
        flight={{
          ...baseFlight,
          status: 'Delayed',
          delayReason: 'Technical',
        }}
      />,
    );
    expect(
      screen.getByLabelText('Flight status: Delayed (Technical)'),
    ).toBeInTheDocument();
  });

  it('disables save when Delayed is selected without a delay reason', async () => {
    const user = userEvent.setup();
    render(<FlightDetails flight={baseFlight} />);
    await user.selectOptions(screen.getByLabelText(/New status/i), 'Delayed');
    expect(screen.getByRole('button', { name: /save status/i })).toBeDisabled();
  });

  it('calls the API with delay reason when Delayed and reason are selected', async () => {
    const user = userEvent.setup();
    updateFlightStatusMock.mockResolvedValue({
      data: {
        ...baseFlight,
        status: 'Delayed',
        delayReason: 'Weather',
        departureTime: baseFlight.departureTime.toISOString(),
        arrivalTime: baseFlight.arrivalTime.toISOString(),
      },
    });
    render(<FlightDetails flight={baseFlight} />);
    await user.selectOptions(screen.getByLabelText(/New status/i), 'Delayed');
    await user.selectOptions(
      screen.getByLabelText(/Delay reason/i),
      'Weather',
    );
    const save = screen.getByRole('button', { name: /save status/i });
    expect(save).not.toBeDisabled();
    await user.click(save);
    expect(updateFlightStatusMock).toHaveBeenCalledWith('1', {
      status: 'Delayed',
      delayReason: 'Weather',
    });
  });
});
