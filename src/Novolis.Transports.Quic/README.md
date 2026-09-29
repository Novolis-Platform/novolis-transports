# Novolis.Transports.Quic

Secure QUIC connection, listener, and stream adapters over
`System.Net.Quic`.

.NET 10 exposes QUIC streams but not QUIC datagrams. Consumers must probe
support and use TCP or an explicitly authenticated UDP channel for
deadline-sensitive datagrams.
