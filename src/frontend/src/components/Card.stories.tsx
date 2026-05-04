import type { Meta, StoryObj } from '@storybook/react-vite';
import Card from './Card';

/**
 * The **Card** component is a flexible container with amber styling used to
 * group related content throughout the application. It renders its
 * `children` inside a padded, rounded panel with a subtle amber divider
 * between child elements.
 *
 * | Prop       | Type        | Default | Description              |
 * |------------|-------------|---------|--------------------------|
 * | `children` | `ReactNode` | —       | Content to render inside |
 */
const meta: Meta<typeof Card> = {
  title: 'Components/Card',
  component: Card,
  parameters: {
    layout: 'padded',
    docs: {
      description: {
        component:
          'Amber-styled content container. Wrap any child content inside a Card to apply consistent padding, rounding and shadow.',
      },
    },
  },
};

export default meta;
type Story = StoryObj<typeof Card>;

/**
 * Card with a simple text paragraph as its child.
 */
export const Default: Story = {
  args: {
    children: <p className="text-amber-900 font-serif">Wright Flyer I — 1903</p>,
  },
};

/**
 * Card containing multiple content sections separated by the amber divider.
 */
export const WithMultipleSections: Story = {
  args: {
    children: (
      <>
        <p className="text-amber-900 font-serif pb-4">Origin: Kitty Hawk, NC</p>
        <p className="text-amber-900 font-serif py-4">Destination: Huffman Prairie, OH</p>
        <p className="text-amber-900 font-serif pt-4">Status: Scheduled</p>
      </>
    ),
  },
};

/**
 * Card with no children — renders an empty panel.
 */
export const Empty: Story = {
  args: {
    children: null,
  },
};
