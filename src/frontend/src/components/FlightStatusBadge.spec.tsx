import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import FlightStatusBadge from './FlightStatusBadge';

describe('FlightStatusBadge', () => {
  it('renders the status label as text', () => {
    render(<FlightStatusBadge status="Landed" />);
    expect(screen.getByText('Landed')).toBeInTheDocument();
  });

  it('applies the green palette for Landed', () => {
    render(<FlightStatusBadge status="Landed" />);
    expect(screen.getByText('Landed')).toHaveClass(
      'bg-green-100',
      'text-green-700',
    );
  });

  it('applies the yellow palette for Delayed', () => {
    render(<FlightStatusBadge status="Delayed" />);
    expect(screen.getByText('Delayed')).toHaveClass(
      'bg-yellow-100',
      'text-yellow-800',
    );
  });

  it('shows delay reason next to Delayed label when provided', () => {
    render(<FlightStatusBadge status="Delayed" delayReason="Weather" />);
    expect(screen.getByText('(Weather)')).toBeInTheDocument();
    expect(
      screen.getByLabelText('Flight status: Delayed (Weather)'),
    ).toBeInTheDocument();
  });

  it('forwards the className prop alongside variant classes', () => {
    render(<FlightStatusBadge status="Landed" className="ml-2" />);
    const badge = screen.getByText('Landed');
    expect(badge).toHaveClass('ml-2');
    expect(badge).toHaveClass('bg-green-100');
  });

  it('renders the human-readable label for InAir', () => {
    render(<FlightStatusBadge status="InAir" />);
    expect(screen.getByText('In air')).toBeInTheDocument();
    expect(screen.getByText('In air')).toHaveClass(
      'bg-indigo-100',
      'text-indigo-700',
    );
  });
});
