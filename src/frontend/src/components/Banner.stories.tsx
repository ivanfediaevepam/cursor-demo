import type { Meta, StoryObj } from '@storybook/react-vite';
import { MemoryRouter } from 'react-router-dom';
import Banner from './Banner';

/**
 * The **Banner** is the hero section displayed at the top of every page.
 * It features the "Dawn of Aviation" headline, an animated airplane, a
 * pulsing sun circle, and a rotating propeller SVG — all powered by CSS
 * animations defined in `index.css`.
 *
 * It accepts no props and relies purely on CSS for its animated state.
 */
const meta: Meta<typeof Banner> = {
  title: 'Components/Banner',
  component: Banner,
  decorators: [
    (Story) => (
      <MemoryRouter>
        <Story />
      </MemoryRouter>
    ),
  ],
  parameters: {
    layout: 'fullscreen',
    docs: {
      description: {
        component:
          'Hero banner for the Aviation Demo app. Uses amber colour palette with CSS micro-animations.',
      },
    },
  },
};

export default meta;
type Story = StoryObj<typeof Banner>;

/**
 * Default appearance of the banner as rendered on the home page.
 */
export const Default: Story = {};
