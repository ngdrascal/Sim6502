Below is a practical, copy-pasteable walkthrough: create the files Wokwi needs, compile AssemblyScript → `.wasm`, and load the chip in Wokwi (with links to the docs and examples you’ll need). Also included is a minimal AssemblyScript example you can extend to talk to Wokwi’s chip API.

**Top-line facts (sources)**

* Wokwi custom chips can be any language that compiles to WebAssembly (C is recommended, but AssemblyScript/Rust/etc. are supported). ([Wokwi Docs][1])
* Pinout & UI controls are defined in a `<chip-name>.chip.json` file. ([Wokwi Docs][2])
* Use `wokwi.toml` (for local / VSCode projects) to tell Wokwi where the `.wasm` binary lives. ([Wokwi Docs][3])
* Wokwi’s chips API (GPIO, timers, SPI, I2C, attributes) is documented in the Custom Chips docs — you’ll follow the same API, but call it from WebAssembly via imports/exports. ([Wokwi Docs][1])
* AssemblyScript compiler docs / import patterns are here — you’ll use `asc` to build `.wasm` and `declare` / `@external` to call host imports. ([AssemblyScript][4])

---

# Quick plan (what you’ll do)

1. Install AssemblyScript and toolchain.
2. Create a Wokwi project with a `*.chip.json` (pins) and `wokwi.toml` that references the compiled `.wasm`.
3. Write an AssemblyScript module that `export`s the functions Wokwi expects (e.g. `chip_init()` etc.) and `declare`s the host functions (Wokwi API) you’ll call.
4. Compile (`asc`) to `.wasm`.
5. Run the simulation on Wokwi (web UI or local VSCode extension). If you use VS Code, `wokwi.toml` tells Wokwi where the chip wasm is. ([Wokwi Docs][3])

---

# File-by-file example (minimal, working skeleton)

Below I give a minimal **working skeleton** you can copy. This example does **not** implement the full pin API (that requires careful pointer/string handling) — it’s a safe first step that shows the build chain and how to export `chip_init`. After this you can extend it to call Wokwi imports (pin_init, pin_read, timers, etc.) — I explain how after the example.

## 1) Project layout

```
my-wokwi-chip/
├─ assembly/                # AssemblyScript sources
│  └─ index.ts
├─ package.json
├─ asconfig.json
├─ build/                   # output (.wasm will go here)
├─ mychip.chip.json
└─ wokwi.toml
```

## 2) `mychip.chip.json` (chip metadata / pins)

```json
{
  "name": "mychip",
  "description": "Minimal AssemblyScript test chip",
  "author": "you",
  "pins": [
    { "name": "VCC", "type": "power" },
    { "name": "GND", "type": "power" },
    { "name": "IN", "type": "digital" },
    { "name": "OUT", "type": "digital" }
  ]
}
```

(Define real pins for your device; docs show full schema and controls/options.) ([Wokwi Docs][2])

## 3) `wokwi.toml` (if running locally / VS Code)

```toml
[wokwi]
version = 1
# If you have a board plus a custom chip, point to firmware as needed.
# For custom chips, add a [[chip]] section:
[[chip]]
name = "mychip"
binary = "build/mychip.chip.wasm"
```

Wokwi will make this chip available to diagrams as `chip-mychip`. ([Wokwi Docs][3])

## 4) AssemblyScript source — `assembly/index.ts` (minimal)

This file demonstrates:

* exporting `chip_init()` so the Wokwi host can call it
* declaring a host `log` function to print to the chips console (you’ll implement later more imports)

```ts
// assembly/index.ts
// Declare host functions. In Wokwi the host provides functions like printf/console.
// Keep them minimal to start — we'll declare a log helper that the host should provide.
@external("env", "chipLog")
declare function chipLog(ptr: usize): void; // we'll pass a string pointer

// helper to call chipLog for a string
export function logString(s: string): void {
  // AssemblyScript automatically places strings in its memory; passing pointer is easy:
  chipLog(changetype<usize>(s));
}

// Export chip_init which Wokwi will call when sim starts.
export function chip_init(): void {
  logString("AssemblyScript chip_init called\n");
  // In a real chip you'd call into imported pin_init(), attr_init(), timers, etc.
}
```

> Implementation note: `@external("env", "chipLog")` tells the compiler "this function will be provided by the host under `env`.`chipLog`". When Wokwi instantiates your wasm it must implement that host import (or use a standard binding like `printf` if provided). Later we’ll show how to wire typical Wokwi API calls (pin_init/pin_read) as imports.

AssemblyScript compiles this to a `.wasm` with `asc`.

## 5) `package.json` + `asconfig.json` (build)

`package.json` (dev-only)

```json
{
  "name": "my-wokwi-chip",
  "devDependencies": {
    "assemblyscript": "^1.0.0"
  },
  "scripts": {
    "asbuild": "asc assembly/index.ts -b build/mychip.chip.wasm -O3 --noAssert"
  }
}
```

`asconfig.json` (optional if you want AssemblyScript config)

```json
{
  "targets": {
    "release": {
      "outFile": "build/mychip.chip.wasm",
      "optimize": true,
      "noAssert": true
    }
  }
}
```

Build:

```bash
npm install
npm run asbuild
# result: build/mychip.chip.wasm
```

(Use `asc` flags as you prefer; docs: AssemblyScript compiler reference.) ([AssemblyScript][4])

## 6) Connect to Wokwi

* Upload the project to Wokwi or open it with the Wokwi VS Code extension.
* If in VS Code, the extension reads `wokwi.toml` and will load `build/mychip.chip.wasm` into the local simulation. If using the web UI, create a Custom Chip via the diagram editor — it will create example files and you can paste your `*.chip.json` and `.wasm` (or use the Wokwi project template). ([Wokwi Docs][3])

---

# How to call Wokwi’s chip API from AssemblyScript (next steps)

Wokwi exposes helper functions for pins, timers, UART, I2C, SPI, attributes (those functions are described in the C API docs). To use them from AssemblyScript you will:

1. **Declare host imports** in AssemblyScript matching the host functions. Example pattern:

   ```ts
   @external("env", "pin_init")
   declare function pin_init(namePtr: usize, mode: u32): u32;
   ```

   You must pass a pointer to the string name (AssemblyScript `changetype<usize>(s)`).

2. **Understand memory & strings.** Host functions will receive pointers into the module’s linear memory. AssemblyScript strings are stored in linear memory — pass their pointer via `changetype<usize>(myString)` or use the exported runtime helpers (see AssemblyScript docs). ([AssemblyScript][5])

3. **Match signatures exactly.** Wokwi’s C API uses `uint32_t`, `pin_t`, `void*` etc. Your AssemblyScript imports must use numeric types (`i32`, `i64`, `usize`) that match expected sizes. If Wokwi expects a pointer to a `pin_watch_config_t`, you’ll likely pass an integer handle instead (the exact ABI depends on how Wokwi implements the host imports). Consult the Wokwi docs for the expected types for each API call. ([Wokwi Docs][1])

4. **Start small.** Implement `chip_init()` that calls `pin_init()` for your pins and `pin_watch()` to register callbacks. Callbacks are more complex because the host calls into your module at runtime — Wokwi will expect exported functions with specific names (e.g., `chip_pin_change`), so export those names from AssemblyScript:

   ```ts
   export function chip_pin_change(user_data: usize, pin: u32, value: u32): void {
     // handle pin change
   }
   ```

   The host must call that export. The docs describe callback names and signatures. ([Wokwi Docs][6])

---

# Pointers & gotchas

* **C is easier** for examples because the Wokwi docs show the C API and there are many C examples. If you don’t need AssemblyScript specifically, start with the C template so you can iterate faster. (Wokwi itself recommends C for now.) ([Wokwi Docs][1])
* **String/pointer ABI** — when calling functions that expect `const char *` names you must pass string pointers (AssemblyScript uses its own string layout). Use `changetype<usize>(s)` to get the pointer and ensure the host expects that layout. If interoperability becomes hairy, you can export a small C shim or use Rust to smooth the ABI, or use AssemblyScript’s raw bindings. ([AssemblyScript][4])
* **Debugging** — Wokwi shows a “Chips Console” where host `printf` / console output can be seen (if you map a host log import). Use that heavily while wiring imports/exports. ([Wokwi Docs][1])

---

# Helpful links / references (again)

* Wokwi custom chips docs (getting started + API): docs.wokwi.com — read the full API (GPIO, timers, SPI, I2C, attributes). ([Wokwi Docs][1])
* Chip JSON format: `*.chip.json` documentation (pin definitions, controls). ([Wokwi Docs][2])
* Local project config: `wokwi.toml` guide (how to point at `.wasm`). ([Wokwi Docs][3])
* Wokwi blog tutorials / examples (CD405x, I2C EEPROM etc.) — follow a C tutorial first and then port to AssemblyScript. ([Wokwi Makers Blog][7])
* AssemblyScript compiler & imports docs (how to `@external` and compile `asc` → `.wasm`). ([AssemblyScript][4])

---

If you want, I’ll do one of the following next (pick one — I’ll produce it right away):

1. Create a **complete** AssemblyScript example that implements a *simple inverter chip* (reads a digital `IN` and writes `OUT`) including the exact import signatures and the `chip_pin_change` export — I’ll try to map the Wokwi API imports into AssemblyScript (I’ll include code and `asc` build commands).
2. Convert a **C tutorial** (e.g., the CD4051/CD4052 blog example) into an AssemblyScript port, with concrete `@external` declarations and a tested build script.
3. Walk through how to debug host imports/exports in Wokwi and show the minimal set of host functions Wokwi provides (so you can wire them into AssemblyScript).

Which one would you like me to build right away? (If you prefer I’ll just generate option 1.)

[1]: https://docs.wokwi.com/chips-api/getting-started?utm_source=chatgpt.com "Getting Started with the Wokwi Custom Chips C API"
[2]: https://docs.wokwi.com/chips-api/chip-json?utm_source=chatgpt.com "Custom Chip Definition (JSON)"
[3]: https://docs.wokwi.com/vscode/project-config?utm_source=chatgpt.com "Configuring Your Project (wokwi.toml)"
[4]: https://www.assemblyscript.org/compiler.html?utm_source=chatgpt.com "Using the compiler"
[5]: https://www.assemblyscript.org/concepts.html?utm_source=chatgpt.com "Concepts | The AssemblyScript Book"
[6]: https://docs.wokwi.com/chips-api/gpio?utm_source=chatgpt.com "GPIO pins API"
[7]: https://blog.wokwi.com/custom-chips-cd4051b-analog-multiplexer/?utm_source=chatgpt.com "Custom Chips? Yes Please"
