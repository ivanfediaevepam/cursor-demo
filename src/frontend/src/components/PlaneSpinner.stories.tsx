import type { Meta, StoryObj } from '@storybook/react-vite';
import PlaneSpinner from './PlaneSpinner';

/**
 * **PlaneSpinner** is an animated loading indicator that uses the
 * `<Airplane />` SVG component driven by GSAP animations.
 *
 * It has three mutually-exclusive states:
 *
 * | State     | Prop                 | Animation           |
 * |-----------|----------------------|---------------------|
 * | Loading   | `isLoading: true`    | Looping flight path |
 * | Success   | `isSuccess: true`    | Fly away off-screen |
 * | Error     | `isError: true`      | Crash + explosion   |
 *
 * | Prop        | Type      | Default   | Description                   |
 * |-------------|-----------|-----------|-------------------------------|
 * | `isLoading` | `boolean` | `false`   | Show looping flight animation |
 * | `isSuccess` | `boolean` | `false`   | Trigger fly-away animation    |
 * | `isError`   | `boolean` | `false`   | Trigger crash animation       |
 */
const meta: Meta<typeof PlaneSpinner> = {
  title: 'Components/PlaneSpinner',
  component: PlaneSpinner,
  parameters: {
    layout: 'centered',
    docs: {
      description: {
        component:
          'GSAP-powered animated loading spinner using the Airplane SVG. Handles three states: loading (looping), success (fly-away), and error (crash).',
      },
    },
  },
  argTypes: {
    isLoading: {
      description: 'When true, the plane loops in a flight path',
      control: 'boolean',
    },
    isSuccess: {
      description: 'When true, the plane flies off-screen',
      control: 'boolean',
    },
    isError: {
      description: 'When true, the plane crashes with an explosion effect',
      control: 'boolean',
    },
  },
};

export default meta;
type Story = StoryObj<typeof PlaneSpinner>;

/**
 * The spinner during an active loading operation — the plane loops continuously.
 */
export const Loading: Story = {
  args: {
    isLoading: true,
    isSuccess: false,
    isError: false,
  },
};

/**
 * The spinner after a successful operation — the plane flies away off-screen.
 */
export const Success: Story = {
  args: {
    isLoading: false,
    isSuccess: true,
    isError: false,
  },
};

/**
 * The spinner after an error — the plane crashes with a debris explosion.
 */
export const Error: Story = {
  args: {
    isLoading: false,
    isSuccess: false,
    isError: true,
  },
};
