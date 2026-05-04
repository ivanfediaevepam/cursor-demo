import type { Meta, StoryObj } from '@storybook/react-vite';
import FlightDetails from './FlightDetails';

/**
 * **FlightDetails** displays the full information card for a single historic
 * flight record, including flight number, aerobatic sequence, route, times,
 * status, fuel data, and log signatures.
 *
 * It also renders an animated `<Airplane />` in the top-right corner and
 * exposes a "Simulate Aerobatic Sequence" button (handler not yet
 * implemented — a great demo target!).
 *
 * | Prop     | Type     | Description                        |
 * |----------|----------|------------------------------------|
 * | `flight` | `Flight` | Complete flight record to display  |
 *
 * Where `Flight` has the following shape:
 * ```ts
 * interface Flight {
 *   id: number;
 *   flightNumber: string;
 *   origin: string;
 *   destination: string;
 *   departureTime: Date;
 *   arrivalTime: Date;
 *   status: string;
 *   fuelRange: number;
 *   fuelTankLeak: boolean;
 *   flightLogSignature: string;
 *   aerobaticSequenceSignature: string;
 * }
 * ```
 */
const meta: Meta<typeof FlightDetails> = {
  title: 'Components/FlightDetails',
  component: FlightDetails,
  parameters: {
    layout: 'padded',
    docs: {
      description: {
        component:
          'Displays detailed information for a single flight. Includes an animated Airplane decoration and a Simulate Aerobatic Sequence button.',
      },
    },
  },
  argTypes: {
    flight: {
      description: 'The complete Flight object to display',
      control: 'object',
    },
  },
};

export default meta;
type Story = StoryObj<typeof FlightDetails>;

const baseFlight = {
  id: 1,
  flightNumber: 'WF-1903-001',
  origin: 'Kitty Hawk, NC',
  destination: 'Huffman Prairie, OH',
  departureTime: new Date('1903-12-17T10:35:00'),
  arrivalTime: new Date('1903-12-17T10:47:00'),
  status: 'Completed',
  fuelRange: 284,
  fuelTankLeak: false,
  flightLogSignature: 'ORV-1903-KH',
  aerobaticSequenceSignature: 'LOOP-ROLL-SPLIT-S',
};

/**
 * A completed, healthy flight — the standard success case.
 */
export const Default: Story = {
  args: {
    flight: baseFlight,
  },
};

/**
 * A flight with an active fuel tank leak — the danger indicator is shown.
 */
export const WithFuelLeak: Story = {
  args: {
    flight: {
      ...baseFlight,
      id: 2,
      flightNumber: 'WF-1903-002',
      fuelTankLeak: true,
      fuelRange: 120,
    },
  },
};

/**
 * A flight that is currently in progress (scheduled status).
 */
export const Scheduled: Story = {
  args: {
    flight: {
      ...baseFlight,
      id: 3,
      flightNumber: 'WF-1904-001',
      status: 'Scheduled',
      origin: 'Huffman Prairie, OH',
      destination: 'Fort Myer, VA',
      departureTime: new Date('1904-05-23T09:00:00'),
      arrivalTime: new Date('1904-05-23T10:30:00'),
    },
  },
};

/**
 * A flight currently airborne.
 */
export const InFlight: Story = {
  args: {
    flight: {
      ...baseFlight,
      id: 4,
      flightNumber: 'WF-1909-001',
      status: 'In Flight',
      origin: 'Fort Myer, VA',
      destination: 'College Park, MD',
      departureTime: new Date('1909-09-09T14:00:00'),
      arrivalTime: new Date('1909-09-09T15:15:00'),
    },
  },
};
