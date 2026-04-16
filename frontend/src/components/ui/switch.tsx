"use client"

import { Switch as SwitchPrimitive } from "@base-ui/react/switch"

import { cn } from "@/lib/utils"

function Switch({
  className,
  size = "default",
  ...props
}: SwitchPrimitive.Root.Props & {
  size?: "sm" | "default"
}) {
  return (
    <SwitchPrimitive.Root
      data-slot="switch"
      data-size={size}
      className={cn(
        "peer group/switch relative inline-flex shrink-0 cursor-pointer items-center rounded-full border-2 transition-colors outline-none",
        "focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2",
        "disabled:cursor-not-allowed disabled:opacity-50",
        "data-[size=default]:h-6 data-[size=default]:w-11",
        "data-[size=sm]:h-5 data-[size=sm]:w-9",
        "data-[checked]:bg-primary data-[checked]:border-primary",
        "data-[unchecked]:bg-input data-[unchecked]:border-input",
        "dark:data-[unchecked]:bg-input/60",
        className
      )}
      {...props}
    >
      <SwitchPrimitive.Thumb
        data-slot="switch-thumb"
        className={cn(
          "pointer-events-none block rounded-full bg-background shadow-md ring-0 transition-transform",
          "group-data-[size=default]/switch:size-5 group-data-[size=sm]/switch:size-4",
          "group-data-[checked]/switch:translate-x-5",
          "group-data-[unchecked]/switch:translate-x-0",
          "group-data-[size=sm]/switch:group-data-[checked]/switch:translate-x-4"
        )}
      />
    </SwitchPrimitive.Root>
  )
}

export { Switch }
