import type { Meta, StoryObj } from '@storybook/react-vite';
import { MemoryRouter } from 'react-router-dom';
import PlaneList from './PlaneList';

/**
 * **PlaneList** renders the aviation collection as a scrollable list.
 * Each item shows the plane's image, name and manufacturing year.
 * Clicking an item triggers a fly-away animation before navigating to the
 * plane's detail page.
 *
 * | Prop     | Type      | Default | Description                              |
 * |----------|-----------|---------|------------------------------------------|
 * | `planes` | `Plane[]` | `[]`    | Array of plane objects to render         |
 *
 * Where `Plane = { id: number; name: string; year: number }`.
 */
const meta: Meta<typeof PlaneList> = {
  title: 'Components/PlaneList',
  component: PlaneList,
  decorators: [
    (Story) => (
      <MemoryRouter>
        <Story />
      </MemoryRouter>
    ),
  ],
  parameters: {
    layout: 'padded',
    docs: {
      description: {
        component:
          'Renders a list of historic aviation planes. Each row is clickable and triggers a fly-away animation before navigating to the detail page.',
      },
    },
  },
  argTypes: {
    planes: {
      description: 'Array of plane objects to display',
      control: 'object',
    },
  },
};

export default meta;
type Story = StoryObj<typeof PlaneList>;

/**
 * A typical list with three planes — the most common view.
 */
export const Default: Story = {
  args: {
    planes: [
      { id: 1, name: 'Wright Flyer I', year: 1903 },
      { id: 2, name: 'Wright Flyer II', year: 1904 },
      { id: 3, name: 'Wright Flyer III', year: 1905 },
    ],
  },
};

/**
 * A single plane — useful for checking layout with minimal data.
 */
export const SinglePlane: Story = {
  args: {
    planes: [{ id: 1, name: 'Wright Flyer I', year: 1903 }],
  },
};

/**
 * An empty list — the component should handle zero items gracefully.
 */
export const EmptyList: Story = {
  args: {
    planes: [],
  },
};

/**
 * A large list with ten planes to verify scrolling behaviour.
 */
export const ManyPlanes: Story = {
  args: {
    planes: [
      { id: 1, name: 'Wright Flyer I', year: 1903 },
      { id: 2, name: 'Wright Flyer II', year: 1904 },
      { id: 3, name: 'Wright Flyer III', year: 1905 },
      { id: 4, name: 'Wright Model A', year: 1907 },
      { id: 5, name: 'Military Flyer', year: 1909 },
      { id: 6, name: 'Transitional Model AB', year: 1909 },
      { id: 7, name: 'Wright Model B', year: 1910 },
      { id: 8, name: 'Wright Model R', year: 1910 },
      { id: 9, name: 'Wright Model EX', year: 1911 },
      { id: 10, name: 'Wright Model C', year: 1912 },
    ],
  },
};
