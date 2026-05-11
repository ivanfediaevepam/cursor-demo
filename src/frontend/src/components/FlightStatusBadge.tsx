import React from "react";
import type { FlightStatus } from "../services/Flight";

interface FlightStatusBadgeProps {
  status: FlightStatus;
  className?: string;
}

const statusStyles: Record<FlightStatus, string> = {
  Scheduled: "bg-slate-100 text-slate-700 ring-slate-200",
  Boarding: "bg-blue-100 text-blue-700 ring-blue-200",
  Departed: "bg-sky-100 text-sky-800 ring-sky-200",
  InAir: "bg-indigo-100 text-indigo-700 ring-indigo-200",
  Landed: "bg-green-100 text-green-700 ring-green-200",
  Cancelled: "bg-red-100 text-red-700 ring-red-200",
  Delayed: "bg-yellow-100 text-yellow-800 ring-yellow-200",
};

const statusLabels: Record<FlightStatus, string> = {
  Scheduled: "Scheduled",
  Boarding: "Boarding",
  Departed: "Departed",
  InAir: "In air",
  Landed: "Landed",
  Cancelled: "Cancelled",
  Delayed: "Delayed",
};

const FlightStatusBadge: React.FC<FlightStatusBadgeProps> = ({
  status,
  className,
}) => {
  const base =
    "inline-flex items-center px-2.5 py-0.5 text-xs font-medium rounded-full ring-1 ring-inset";
  return (
    <span
      className={`${base} ${statusStyles[status]} ${className ?? ""}`}
      aria-label={`Flight status: ${statusLabels[status]}`}
    >
      {statusLabels[status]}
    </span>
  );
};

export default FlightStatusBadge;
