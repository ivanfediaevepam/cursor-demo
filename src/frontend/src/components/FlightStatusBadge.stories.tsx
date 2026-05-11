import type { Meta, StoryObj } from '@storybook/react-vite';
import FlightStatusBadge from './FlightStatusBadge';
import type { FlightStatus } from '../services/Flight';

const STATUSES: FlightStatus[] = [
  'Scheduled',
  'Boarding',
  'Departed',
  'InAir',
  'Landed',
  'Cancelled',
  'Delayed',
];

/**
 * **FlightStatusBadge** shows a flight lifecycle state as a compact pill.
 * Background and text colors follow the status (e.g. green for landed,
 * yellow for delayed).
 *
 * | Prop        | Type           | Default | Description                          |
 * |-------------|----------------|---------|--------------------------------------|
 * | `status`    | `FlightStatus` | —       | Which variant colors/label to show   |
 * | `className` | `string`       | —       | Optional extra Tailwind classes      |
 */
const meta = {
  title: 'Components/FlightStatusBadge',
  component: FlightStatusBadge,
  tags: ['autodocs'],
  parameters: {
    layout: 'centered',
    docs: {
      description: {
        component:
          'Pill badge for a single `FlightStatus` value. Uses Tailwind palette tokens consistent with list cards in the app.',
      },
    },
  },
  argTypes: {
    status: {
      description: 'The flight status the badge represents.',
      control: 'select',
      options: STATUSES,
    },
    className: {
      description: 'Extra Tailwind classes merged onto the badge root.',
      control: 'text',
    },
  },
} satisfies Meta<typeof FlightStatusBadge>;

export default meta;
type Story = StoryObj<typeof meta>;

/** Neutral slate — flight is on the schedule, not yet boarding. */
export const Scheduled: Story = { args: { status: 'Scheduled' } };

/** Blue — passengers are boarding. */
export const Boarding: Story = { args: { status: 'Boarding' } };

/** Sky — aircraft has departed. */
export const Departed: Story = { args: { status: 'Departed' } };

/** Indigo — en route (label reads “In air”). */
export const InAir: Story = { args: { status: 'InAir' } };

/** Green — flight has landed. */
export const Landed: Story = { args: { status: 'Landed' } };

/** Red — flight cancelled. */
export const Cancelled: Story = { args: { status: 'Cancelled' } };

/** Yellow / amber — delayed. */
export const Delayed: Story = { args: { status: 'Delayed' } };

/** Every status in one column for quick palette comparison (not a substitute for isolated stories above). */
export const AllStatuses: Story = {
  parameters: { layout: 'padded' },
  render: () => (
    <div className="flex flex-col gap-2">
      {STATUSES.map((s) => (
        <div key={s} className="flex items-center gap-3">
          <FlightStatusBadge status={s} />
          <span className="text-sm text-gray-600">{s}</span>
        </div>
      ))}
    </div>
  ),
};
